# タスク間依存関係

このドキュメントは、プロジェクト「ログ送信アプリケーション」の各実装タスク間の依存関係と、見積もり工数に基づいたスケジュールを示します。
タスクの開始日は2025年5月13日を基準とし、各タスクの期間は割り当てられたストーリーポイントに基づいて設定されています（1SP≒1日、2SP≒2日、3SP≒3日としてガントチャート上に表現）。

```mermaid
gantt
    dateFormat YYYY-MM-DD
    title タスク依存関係ガントチャート
    excludes    weekends

    %% セクション定義
    section 初期セットアップとコアモデル
    プロジェクトセットアップ           :T0001, 2025-05-13, 1d
    DI設定                          :T0002, after T0001, 1d
    設定情報読み込み                  :T0003, after T0002, 1d
    MainWindow UI基本レイアウト       :T0004, after T0001, 1d
    LogEntryクラスとEnum実装          :T0005, after T0001, 1d
    LogFileHandler(パス管理,日付)     :T0007, after T0001, 2d

    section サービスとファクトリ
    LogEntryFactory実装             :T0006, after T0005, 2d
    LogFileHandler(ログ追記)          :T0008, after T0005 T0007, 2d
    DialogService実装               :T0013, after T0002, 1d
    IoTHubClient(IF,モック)         :T0014, after T0003, 2d
    LogWriteService実装             :T0009, after T0006 T0008, 1d

    section ViewModelとUIロジック
    MainWindow ViewModel実装        :T0010, after T0009, 2d
    ボタンクリックとVM連携            :T0011, after T0004 T0010, 1d
    UIブロッキング実装                :T0012, after T0004 T0010, 1d

    section ファイルアップロード処理
    FileUploadService(基本)         :T0015, after T0003 T0007 T0014, 2d
    FileUploadService(リトライ)       :T0016, after T0015, 2d

    section 統合、最終化、テスト
    MainWindowとFileUpload連携      :T0017, after T0010 T0012 T0015, 1d
    MainWindowとDialogService連携   :T0018, after T0010 T0013 T0017, 1d
    IoTHubClient実装(SDK)           :T0019, after T0003 T0014 T0016, 3d
    アプリケーション終了処理            :T0020, after T0001, 1d
    単体テスト                        :T0021, after T0009 T0016 T0019, 3d
```

**凡例:**

- `Txxxx`: タスク番号 (例: `T0001` はタスク0001を指します)
- `日付, Xd`: タスクの開始日と期間 (日数)
- `after Txxxx`: 指定したタスク完了後に開始可能であることを示します

このガントチャートは、タスクの実行順序と依存関係を視覚的に把握するのに役立ちます。
週末は作業日としてカウントされていません。
