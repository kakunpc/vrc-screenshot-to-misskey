# 1.2.0

- ロガーの追加
- 同名のファイルをアップロードするか選べるように変更
- XSOverlay連携の対応
- httpのドメインでも動作するように変更
- アップロード失敗時のリトライにサーバーに負荷をかけないようにBackoffの対応

# 1.1.2

- 最終時刻の保存時間が間違っていたのを修正

# 1.1.0

- ファイル名に日付がある場合はその日付を優先するように変更（ない場合は作成時刻を採用）
- アップロード時にAVIFに変換するオプションを追加
- 日付を過ぎてから何時間まで前日とみなすかのオプションを追加
- `srcDir` を `src_dir` のスネークケースに変更

# 1.0.0

- 初版