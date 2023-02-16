
rd /s /q output
dotnet publish --output output\vrc-screenshot-to-misskey

del /q output\vrc-screenshot-to-misskey\vrc-screenshot-to-misskey.pdb
copy config.json.sample output\vrc-screenshot-to-misskey\config.json

Powershell Compress-Archive -Path output\vrc-screenshot-to-misskey -DestinationPath vrc-screenshot-to-misskey.zip