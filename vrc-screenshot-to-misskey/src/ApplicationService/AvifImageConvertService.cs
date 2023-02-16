using HeifEncoderSample;
using LibHeifSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using vrc_screenshot_to_misskey.Domain;
using Image = SixLabors.ImageSharp.Image;

namespace vrc_screenshot_to_misskey.ApplicationService;

public sealed class AvifImageConvertService : IDisposable
{
    private readonly IApplicationConfigRepository _applicationConfigRepository;
    private readonly string _outputPath;

    public AvifImageConvertService(IApplicationConfigRepository applicationConfigRepository)
    {
        _applicationConfigRepository = applicationConfigRepository;
        var random = new Random();
        var randomName = random.Next(0, 10000).ToString();
        var tempPath = Path.GetTempPath();
        _outputPath = Path.Combine(tempPath, $"screenshot_to_misskey_temp_{randomName}.avif");
    }

    public void Dispose()
    {
        if (File.Exists(_outputPath)) File.Delete(_outputPath);
    }

    public async Task<string> Run(string inputPath)
    {
        var applicationConfig = await _applicationConfigRepository.FindAsync();
        // 変換オプションが有効か
        if (!applicationConfig.UseAvifConvert)
        {
            return inputPath;
        }
        
        return await HeifImageConvert(inputPath);
    }

    // https://github.com/0xC0000054/libheif-sharp-samples

    #region libheif-Converter

    private Task<string> HeifImageConvert(string inputPath)
    {
        return Task.Run(() =>
        {
            try
            {
                using var context = new HeifContext();
                var format = HeifCompressionFormat.Av1;
                var encoderDescriptors = context.GetEncoderDescriptors(format);
                if (encoderDescriptors == null) return inputPath;
                var encoderDescriptor = encoderDescriptors[0];

                using var encoder = context.GetEncoder(encoderDescriptor);
                using var heifImage = CreateHeifImage(inputPath, false, false, false, out var metadata);
                encoder.SetLossyQuality(100);

                var encodingOptions = new HeifEncodingOptions
                {
                    SaveAlphaChannel = true,
                    WriteTwoColorProfiles = false
                };

                if (metadata.ExifProfile is null
                    && metadata.XmpProfile is null)
                {
                    context.EncodeImage(heifImage, encoder, encodingOptions);
                }
                else
                {
                    using var imageHandle =
                        context.EncodeImageAndReturnHandle(heifImage, encoder, encodingOptions);
                    if (metadata.ExifProfile != null)
                    {
                        context.AddExifMetadata(imageHandle, metadata.ExifProfile.ToByteArray());
                    }

                    if (metadata.XmpProfile != null)
                    {
                        context.AddXmpMetadata(imageHandle, metadata.XmpProfile.ToByteArray());
                    }
                }

                context.WriteToFile(_outputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return inputPath;
            }

            return _outputPath;
        });
    }


    static HeifImage CreateHeifImage(string inputPath,
        bool lossless,
        bool writeTwoColorProfiles,
        bool premultiplyAlpha,
        out ImageMetadata metadata)
    {
        HeifImage heifImage = null;
        HeifImage temp = null;

        try
        {
            if (ImageMayHaveTransparency(inputPath))
            {
                var image = Image.Load<Rgba32>(inputPath);
                metadata = image.Metadata;

                temp = ImageConversion.ConvertToHeifImage(image, premultiplyAlpha);

                if (temp.HasAlphaChannel && premultiplyAlpha)
                {
                    temp.IsPremultipliedAlpha = true;
                }
            }
            else
            {
                var image = Image.Load<Rgb24>(inputPath);
                metadata = image.Metadata;

                temp = ImageConversion.ConvertToHeifImage(image);
            }

            if (writeTwoColorProfiles && metadata.IccProfile != null)
            {
                temp.IccColorProfile = new HeifIccColorProfile(metadata.IccProfile.ToByteArray());

                if (lossless)
                {
                    // The Identity matrix coefficient places the RGB values into the YUV planes without any conversion.
                    // This reduces the compression efficiency, but allows for fully lossless encoding.
                    temp.NclxColorProfile = new HeifNclxColorProfile(ColorPrimaries.BT709,
                        TransferCharacteristics.Srgb,
                        MatrixCoefficients.Identity,
                        fullRange: true);
                }
                else
                {
                    temp.NclxColorProfile = new HeifNclxColorProfile(ColorPrimaries.BT709,
                        TransferCharacteristics.Srgb,
                        MatrixCoefficients.BT601,
                        fullRange: true);
                }
            }
            else
            {
                if (lossless)
                {
                    // The Identity matrix coefficient places the RGB values into the YUV planes without any conversion.
                    // This reduces the compression efficiency, but allows for fully lossless encoding.
                    temp.NclxColorProfile = new HeifNclxColorProfile(ColorPrimaries.BT709,
                        TransferCharacteristics.Srgb,
                        MatrixCoefficients.Identity,
                        fullRange: true);
                }
                else if (metadata.IccProfile != null)
                {
                    temp.IccColorProfile = new HeifIccColorProfile(metadata.IccProfile.ToByteArray());
                }
                else
                {
                    temp.NclxColorProfile = new HeifNclxColorProfile(ColorPrimaries.BT709,
                        TransferCharacteristics.Srgb,
                        MatrixCoefficients.BT601,
                        fullRange: true);
                }
            }

            heifImage = temp;
            temp = null;
        }
        finally
        {
            temp?.Dispose();
        }

        return heifImage;
    }

    static bool ImageMayHaveTransparency(string path)
    {
        bool mayHaveTransparency = true;

        var imageInfo = Image.Identify(path, out var imageFormat);

        if (imageFormat is PngFormat)
        {
            var pngMeta = imageInfo.Metadata.GetPngMetadata();

            mayHaveTransparency = pngMeta.HasTransparency;
        }
        else if (imageFormat is JpegFormat)
        {
            mayHaveTransparency = false;
        }
        else if (imageFormat is BmpFormat)
        {
            var bmpMeta = imageInfo.Metadata.GetBmpMetadata();

            mayHaveTransparency = bmpMeta.BitsPerPixel == BmpBitsPerPixel.Pixel32;
        }
        else if (imageFormat is TgaFormat)
        {
            var tgaMeta = imageInfo.Metadata.GetTgaMetadata();

            mayHaveTransparency = tgaMeta.BitsPerPixel == TgaBitsPerPixel.Pixel32
                                  && tgaMeta.AlphaChannelBits != 0;
        }

        return mayHaveTransparency;
    }

    #endregion
}