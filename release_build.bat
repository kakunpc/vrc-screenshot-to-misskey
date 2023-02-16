
rd /s /q output
dotnet publish --output output\vrc-screenshot-to-misskey
del /q vrc-screenshot-to-misskey.zip

del /q output\vrc-screenshot-to-misskey\vrc-screenshot-to-misskey.pdb
copy config.json.sample output\vrc-screenshot-to-misskey\config.json

copy COPYING.md output\vrc-screenshot-to-misskey\COPYING.md
copy LICENSE.md output\vrc-screenshot-to-misskey\LICENSE.md
copy README.md output\vrc-screenshot-to-misskey\README.md

Powershell Compress-Archive -Path output\vrc-screenshot-to-misskey -DestinationPath vrc-screenshot-to-misskey.zip