# Changelog

ChainOSC for Windowsの主な変更履歴を記録します。

形式は[Keep a Changelog](https://keepachangelog.com/ja/1.1.0/)を参考にし、バージョン番号には[Semantic Versioning](https://semver.org/lang/ja/)を使用します。

## [Unreleased]

### Added

- GitHub Pages用のドキュメントポータルを追加
- 日本語／英語のクイックスタートガイドを追加
- 日本語／英語のユーザーガイドと最新Releaseへの導線を追加

## [1.0.0] - 2026-08-21

### Changed

- ChainOSC for Windowsの最初の安定版として正式リリース
- アプリ、設定バックアップ、配布物のバージョン表記を`1.0.0`へ統一
- 現行のTauri版と旧.NET/WPFプロトタイプを明確にするため、旧実装を`legacy/dotnet-prototype/`へ移動

## [0.7.0]

### Added

- Web UIの英語／日本語切り替えと選択言語の保存に対応
- 各Keyの「…」メニューを追加し、プリセットのエクスポート／インポートとKey削除を集約
- グローバルホットキー未設定時の保存確認と、未設定のまま保存する機能を追加
- 日本語ユーザーガイドとM5ChainOSC共有Keyプリセットへの案内を追加
- MITライセンス、第三者ライセンス、MPL-2.0依存ソースの入手先を整備
- タグからWindowsポータブルZIPとSHA-256チェックサムを生成するGitHub Actionsを追加
- 正式なChainOSCアプリアイコンを追加

### Changed

- UI用語をM5ChainOSC／ChainOSCminiへ統一
- 自動起動を有効にした状態でアップデート／削除する場合の注意をUIとユーザーガイドへ追加
- ルートREADMEの実行・配布手順をTauri版へ更新

## [0.6.0]

### Added

- ChainOSC for Windows全体設定のJSONエクスポート／インポートに対応
- 未保存状態の表示を追加
- 製品名、バージョン、実行環境を示すシステム情報を追加
- アプリケーション、ウィンドウ、タスクトレイ用のカスタムアイコンに対応

### Changed

- JSONエクスポート時に保存先を選択し、完了結果を画面へ通知する方式へ変更

## [0.5.0]

### Added

- Windowsへのサインイン時に自動起動する機能を追加
- 自動起動時に設定画面を表示せずタスクトレイで起動する機能を追加
- ChainOSCの複数起動を防止し、2回目の起動で既存画面を表示する機能を追加

## [0.4.0]

### Added

- ウィンドウの最小化／閉じる操作でタスクトレイへ格納する機能を追加
- タスクトレイアイコンのダブルクリックによる再表示に対応
- トレイメニューからの設定画面表示と完全終了に対応
- 設定画面を非表示にした状態でのホットキーとOSC送信に対応
- セットアップ不要で利用できるポータブルexeのReleaseビルドに対応

## [0.3.0]

### Added

- 1つのKeyへ押した時／離した時の合計8件までOSCメッセージを設定する機能を追加
- OSCメッセージの追加、削除、並べ替え、0件設定に対応
- Keyのシーケンスモードと開始値／終了値／増減量／型の設定に対応
- M5ChainOSC／ChainOSCmini互換の`ChainOSC-device-preset`形式に対応
- 旧`M5ChainOSC-device-preset`形式のインポート互換性を追加
- KeyプリセットのJSONエクスポート／インポートに対応

## [0.2.0]

### Added

- 任意の数のKey追加／削除に対応
- Keyごとのデバイス名、グローバルホットキー、OSC設定に対応
- 設定の保存とアプリ再起動後の復元に対応
- グローバルホットキーの重複検証を追加

## [0.1.0]

### Added

- Windowsグローバルホットキーの押下／解放によるOSC送信に対応
- OSC送信先ホスト、UDPポート、OSC Address、型、値の設定に対応
- Int、Float、String形式のOSCメッセージに対応
- 押した時／離した時のテスト送信と動作履歴を追加

[Unreleased]: https://github.com/shimez/ChainOSC-for-Windows/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v1.0.0
[0.7.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.7.0
[0.6.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.6.0
[0.5.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.5.0
[0.4.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.4.0
[0.3.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.3.0
[0.2.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.2.0
[0.1.0]: https://github.com/shimez/ChainOSC-for-Windows/releases/tag/v0.1.0
