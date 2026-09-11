---
layout: default
title: ChainOSC for Windows クイックスタート
permalink: /quick-start/
---

# ChainOSC for Windows クイックスタート

[English version](../en/quick-start/)

このガイドでは、ChainOSC for Windowsの入手から、グローバルホットキーを使ってVRChatへOSCメッセージを送信するまでを説明します。各機能の詳細は[日本語ユーザーガイド](../user-guide/)を参照してください。

> [!IMPORTANT]
> ChainOSC for Windowsは個人が開発する非公式プロジェクトです。VRChat Inc.、M5Stack Technology Co., Ltd.および各関連企業による公式製品ではありません。

## 用意するもの

- Windows 10またはWindows 11（64bit）のPC
- OSCを利用するアプリケーション（このガイドではVRChat）
- Microsoft Edge WebView2 Runtime

WebView2 Runtimeは一般的なWindows 10／11環境に導入されています。アプリが起動しない場合は、Microsoftから最新版を導入してください。

## 1. ChainOSC for Windowsを入手する

1. [最新のGitHub Release](https://github.com/shimez/ChainOSC-for-Windows/releases/latest)を開きます。
2. Assetsから`ChainOSC-for-Windows-vX.Y.Z-win-x64-portable.zip`をダウンロードします。
3. ZIPを任意のフォルダーへ展開します。
4. 展開したフォルダー内の`chainosc-for-windows.exe`を起動します。

ZIPの中から直接exeを起動せず、先にすべてのファイルを展開してください。Windows Defender SmartScreenが表示された場合は、配布元とファイル名を確認してから実行してください。

## 2. VRChatでOSCを有効にする

VRChatのリングメニューから次の順に開き、OSCを有効にします。

```text
リングメニュー → オプション → OSC → 有効
```

## 3. OSC送信先を設定する

同じPCで動作するVRChatへ送る場合は、次の値を使用します。

- ホスト名またはIPアドレス：`127.0.0.1`
- UDPポート：`9000`

別のPCや機器へ送る場合は、受信側のIPアドレスとUDPポートを指定してください。

## 4. Key 1個とホットキーを設定する

1. Keyの「デバイス名」に用途が分かる名前を入力します。
2. 「グローバルホットキー」の入力欄を選択します。
3. 使用するキー、または`Ctrl`、`Alt`、`Shift`、Windowsキーを含むキーの組み合わせを押します。
4. 「押した時」のOSCアドレス、型、値を設定します。

VRChat Voiceの例：

- OSCアドレス：`/input/Voice`
- 型：`Int`
- 値：`1`

5. 「離した時」に切り替えてOSCアドレス、型、値を設定します。

VRChat Voiceの例：

- OSCアドレス：`/input/Voice`
- 型：`Int`
- 値：`0`

## 5. 送信をテストして保存する

1. 「押した時をテスト」と「離した時をテスト」で動作を確認します。
2. 画面下部の「デバッグログ」を開き、送信したOSCアドレス、型、値が表示されることを確認します。
3. 「すべての設定を保存」を押します。
4. 設定したグローバルホットキーを押して、VRChatのVoice入力状態が有効になることを確認します。
5. グローバルホットキーを離して、Voice入力状態が元に戻ることを確認します。

## 6. タスクトレイで使用する

ウィンドウを閉じるか最小化すると、ChainOSC for Windowsは終了せずタスクトレイへ格納されます。設定画面が非表示でもホットキーとOSC送信は動作します。

- タスクトレイアイコンをダブルクリック：設定画面を表示
- `Show ChainOSC`：設定画面を表示
- `Exit`：アプリを完全に終了

以上で基本設定は完了です。複数OSCメッセージ、Sequenceモード、Device Preset、全体バックアップ、自動起動などは[日本語ユーザーガイド](../user-guide/)を参照してください。設定の再利用や他のChainOSCシリーズとの共有にはDevice Presetを利用できます。
