# これはなに？

VRChatで取った写真をMisskeyに送信します。  
単純にドライブにアップロードし続けるので容量には気をつけてください。

# インストールと使い方

ファイルをダウンロードして展開する

https://github.com/kakunpc/vrc-screenshot-to-misskey/releases

## 設定を変更する

config.jsonをテキスト開いて編集します。

```
{
  "domain": "misskey.io",
  "token": "",
  "upload_path": "VRChat/{YYYY}-{MM}-{DD}",
  "src_dir": "C:\\Users\\ユーザー名\\Pictures\\VRChat",
  "use_avif_convert": false,
  "time_to_previous_day": 5
}
```

- domain: アップロード先がmisskey.io以外の場合は変更してください。
- token: アクセストークンが保存されます（扱いには十分注意してください）
- upload_path: アップロード先のフォルダを指定できます。
- src_dir: VRChatの写真の保存先を指定してください。（空の場合は自動で設定されます）
- use_avif_convert: avifに変換してからアップロードするオプション（有効の場合容量の節約が期待できます）
- time_to_previous_day: 日付を過ぎてから何時間まで前日とみなすか[0-24]（1だと、25時まで当日のフォルダに入れるようになります）

## 起動

vrc-screenshot-to-misskey.exe をダブルクリックで起動してください。

## 終了方法

画面右下のタスクバーにアイコンがあるので、右クリック→終了で監視を終了できます。

# トラブルシューティング

## 起動しない

必要なライブラリがインストールされてない場合があります。  

https://dotnet.microsoft.com/ja-jp/download/dotnet/6.0

より、`ASP.NET Core ランタイム 6.0.13` `.NET デスクトップ ランタイム 6.0.13` `.NET Runtime 6.0.13` をインストールしてください。

## ファイルがアップロードされてない、途中で落ちちゃった

まだあまりきちんと整備してないためAPIの制限が厳しいインスタンスではエラーが発生して強制終了してしまう可能性があります。  
再度アプリケーションを起動すると途中から再開するのでしばらく待ってから起動してください。

# こういう機能が欲しい・バグを見つけた

https://github.com/kakunpc/vrc-screenshot-to-misskey/issues/new

こちらより報告をお願いします。書き方に特に指定はないので日本語で大丈夫です。


# 権利表記

権利表記は [こちら](COPYING.md)