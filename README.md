# これはなに？

VRChatで取った写真をMisskeyに送信します。  
単純にドライブにアップロードし続けるので容量には気をつけてください。

# インストールと使い方

ファイルをダウンロードして展開する

https://github.com/kakunpc/vrc-screenshot-to-misskey/releases

## 設定を変更する

```
{
  "domain": "misskey.io",
  "token": "",
  "upload_path": "VRChat/{YYYY}-{MM}-{DD}",
  "srcDir": "C:\\Users\\ユーザー名\\Pictures\\VRChat"
}
```

domain: アップロード先がmisskey.io以外の場合は変更してください。
upload_path: アップロード先のフォルダを指定できます。
srcDir: VRChatの写真の保存先を指定してください。

## 起動

vrc-screenshot-to-misskey.exe をダブルクリックで起動してください。

## 終了方法

画面右下のタスクバーにアイコンがあるので、右クリック→終了で監視を終了できます。

# ライセンス表記

### Json.Net

https://www.newtonsoft.com/json

The MIT License (MIT)

Copyright (c) 2007 James Newton-King

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


--------------------------------------------------------------------

### MisskeyDotNet

https://github.com/EbiseLutica/Misskey.NET

The MIT License (MIT)

Copyright (c) 2020 Xeltica

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.