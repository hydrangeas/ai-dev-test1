# 静的モデリング

## シーケンス図

### シーケンス図 <ログ書き込みとアップロード処理>

```mermaid
sequenceDiagram
    actor 一般ユーザー
    participant MainWindow as "MainWindow (UI)"
    participant LogWriteService as "LogWriteService"
    participant LogFileHandler as "LogFileHandler"
    participant FileUploadService as "FileUploadService"
    participant IoTHubClient as "IoTHubClient"
    participant DialogService as "DialogService"

    一般ユーザー->>MainWindow: ログ書き込みボタン押下
    MainWindow->>MainWindow: SetUIBlocking(true)
    MainWindow->>LogWriteService: PressLogWriteButton()
    LogWriteService->>LogFileHandler: AppendLogEntry(logEntry)
    LogFileHandler-->>LogWriteService: LogWrittenToFile (イベント)
    LogWriteService-->>MainWindow: (ログ書き込み完了)

    MainWindow->>FileUploadService: UploadFile(authInfo)
    
    loop 最大3回 (FileUploadServiceが内部で試行回数を管理)
        FileUploadService->>IoTHubClient: GetFileUploadSasUriAsync(blobName)
        alt SAS URI取得成功
            IoTHubClient-->>FileUploadService: SasUri
            FileUploadService->>IoTHubClient: UploadToBlobAsync(sasUri, logFileContent)
            alt アップロード成功
                IoTHubClient-->>FileUploadService: FileUploaded (イベント)
                FileUploadService-->>DialogService: ShowSuccessDialog()
                DialogService-->>MainWindow: (成功ダイアログ表示指示)
                MainWindow-->>一般ユーザー: 成功ダイアログ表示
                一般ユーザー->>MainWindow: OKボタン押下 (ダイアログ)
                MainWindow-->>DialogService: (ダイアログクローズ)
            else アップロード失敗 (Blob Storageへのアップロード自体)
                IoTHubClient-->>FileUploadService: FileUploadFailed (イベント)
                FileUploadService-->>DialogService: ShowFailureDialog()
                DialogService-->>MainWindow: (失敗ダイアログ表示指示)
                MainWindow-->>一般ユーザー: 失敗ダイアログ表示
                一般ユーザー->>MainWindow: OKボタン押下 (ダイアログ)
                MainWindow-->>DialogService: (ダイアログクローズ)
            end
            %% FileUploadServiceはSAS URI取得成功のため、これ以上のループは行わないと判断
        else SAS URI取得失敗 (またはタイムアウト)
            IoTHubClient-->>FileUploadService: FileUploadFailed (イベント)
            alt FileUploadServiceが最終試行と判断
                FileUploadService-->>DialogService: ShowFailureDialog()
                DialogService-->>MainWindow: (失敗ダイアログ表示指示)
                MainWindow-->>一般ユーザー: 失敗ダイアログ表示
                一般ユーザー->>MainWindow: OKボタン押下 (ダイアログ)
                MainWindow-->>DialogService: (ダイアログクローズ)
            else FileUploadServiceがまだ試行可能と判断
                FileUploadService->>FileUploadService: 5秒待機
            end
        end
    end
    MainWindow->>MainWindow: SetUIBlocking(false)
```

**説明:**

1.  一般ユーザーがログ書き込みボタンを押すと、UIがブロッキングされます。
2.  `LogWriteService` が `LogFileHandler` を使ってログエントリをファイルに追記します。
3.  ログ書き込み完了後、`FileUploadService` がアップロード処理を開始します。
4.  `FileUploadService` は `IoTHubClient` を介して最大3回SAS URIの取得を試みます。
    *   SAS URI取得に成功すると、Blob Storageへのアップロードを試行し、結果に応じて成功または失敗ダイアログを表示します。この時点でリトライは終了します。
    *   SAS URI取得に失敗した場合、最終試行でなければ5秒待機してリトライします。最終試行でも失敗した場合は失敗ダイアログを表示します。
5.  全ての処理が完了後、UIのブロッキングが解除されます。

### シーケンス図 <アプリケーション終了処理>

```mermaid
sequenceDiagram
    actor 一般ユーザー
    participant MainWindow as "MainWindow (UI)"
    participant Application as "Application (WPF)"

    一般ユーザー->>MainWindow: ×ボタン押下
    MainWindow->>Application: Shutdown()
    Application-->>一般ユーザー: (アプリケーション終了)
```

**説明:**

1.  一般ユーザーが×ボタンを押すと、`MainWindow` がWPFの `Application` オブジェクトに終了を指示します。
2.  アプリケーションが終了します。

## ステートマシン図

```mermaid
stateDiagram-v2
    [*] --> Idle : アプリケーション起動
    Idle --> ProcessingLog : ログ書き込みボタン押下
    ProcessingLog --> Uploading : ログ書き込み完了
    ProcessingLog --> Idle : ログ書き込み失敗 (I/Oエラー等) / ダイアログOK
    Uploading --> Idle : アップロード成功 / ダイアログOK
    Uploading --> Idle : アップロード失敗 / ダイアログOK
    Uploading --> Idle : SAS URI取得最終失敗 / ダイアログOK

    state ProcessingLog {
        [*] --> WritingLog
        WritingLog --> LogWriteComplete : 成功
        WritingLog --> LogWriteFailed : 失敗
        LogWriteComplete --> [*]
        LogWriteFailed --> [*]
    }

    state Uploading {
        [*] --> GettingSasUri
        GettingSasUri --> UploadingToBlob : 取得成功
        GettingSasUri --> SasUriFailed : 取得失敗
        UploadingToBlob --> UploadSucceeded : 成功
        UploadingToBlob --> UploadFailedToBlob : 失敗
        
        SasUriFailed --> GettingSasUri : リトライ (3回まで)
        SasUriFailed --> [*] : 最終失敗
        UploadSucceeded --> [*]
        UploadFailedToBlob --> [*]
    }

    Idle --> [*] : ×ボタン押下
    ProcessingLog --> [*] : ×ボタン押下 (UIブロッキング中のため実質操作不可)
    Uploading --> [*] : ×ボタン押下 (UIブロッキング中のため実質操作不可)
```

**説明:**

*   アプリケーションは主に `Idle` (待機中)、`ProcessingLog` (ログ処理中)、`Uploading` (アップロード中) の状態を取ります。
*   ユーザー操作や処理結果に応じて状態が遷移します。
*   `ProcessingLog` および `Uploading` 状態は、UIブロッキングによりユーザーが×ボタンを操作できない想定です。

## クラス図

```mermaid
classDiagram
    direction LR

    class MainWindow {
        +LogWriteService logWriteService
        +FileUploadService fileUploadService
        +DialogService dialogService
        +void LogWriteButton_Click()
        +void SetUIBlocking(bool isBlocking)
        +void ShowSuccessDialog(string message)
        +void ShowFailureDialog(string message)
        +void OnApplicationShutdown()
    }

    class LogWriteService {
        +LogFileHandler logFileHandler
        +LogEntryFactory logEntryFactory
        +PressLogWriteButton() LogWrittenToFileEvent
    }

    class LogFileHandler {
        -string currentFilePath
        -DateTime currentDate
        +AppendLogEntry(LogEntry logEntry)
        +GetLogFileContentForUpload(string filePath) byte[]
        +GetCurrentLogFilePath() string
        -EnsureLogFileForToday()
    }

    class LogEntry {
        +DateTimeOffset Timestamp
        +EventType EventType
        +string Comment
        +string ToJsonLine()
    }

    class EventType {
        <<enumeration>>
        START
        STOP
        WARN
        ERROR
    }
    note for EventType "イベント種別 (START, STOP, WARN, ERROR)"

    class LogEntryFactory {
        +CreateLogEntry(EventType eventType) LogEntry
        -GetRandomComment(EventType eventType) string
    }
    LogEntryFactory ..> LogEntry : creates/returns

    class FileUploadService {
        +IoTHubClient iotHubClient
        +LogFileHandler logFileHandler
        +UploadFile(AuthenticationInfo authInfo) FileUploadResultEvent
        -int retryCount
    }

    class IoTHubClient {
        -string connectionString
        +GetFileUploadSasUriAsync(string blobName) SasUriResult
        +UploadToBlobAsync(string sasUri, byte[] fileContent) UploadToBlobResult
        +NotifyFileUploadCompleteAsync(string correlationId, bool isSuccess)
    }

    class DialogService {
        +ShowSuccess(string title, string message)
        +ShowFailure(string title, string message)
        +ShowError(string title, string message)
    }

    class AppSettings {
        +AuthenticationInfo AuthInfo
        +LoadAppSettings() AppSettings
    }

    class AuthenticationInfo {
        +string DeviceId
        +string ConnectionString
    }

    %% Relationships
    MainWindow ..> LogWriteService : uses
    MainWindow ..> FileUploadService : uses
    MainWindow ..> DialogService : uses
    MainWindow ..> AppSettings : uses

    LogWriteService ..> LogFileHandler : uses
    LogWriteService ..> LogEntryFactory : uses
    LogEntry ..> EventType : uses

    FileUploadService ..> IoTHubClient : uses
    FileUploadService ..> AuthenticationInfo : uses
    FileUploadService ..> LogFileHandler : uses

    %% Events (conceptual)
    class LogWrittenToFileEvent { +string FilePath }
    class FileUploadResultEvent { +bool IsSuccess; +string Message }
    class SasUriResult { +bool IsSuccess; +string SasUri; +string CorrelationId; +string ErrorMessage }
    class UploadToBlobResult { +bool IsSuccess; +string ErrorMessage }

    %% Notes
    note for MainWindow "UIロジック、イベントハンドリング"
    note for LogWriteService "ログ書き込みのユースケース担当"
    note for LogFileHandler "ログファイルのパス管理、追記、日付変更時のファイル切り替え、内容読み込み担当"
    note for LogEntry "単一のログ記録"
    note for LogEntryFactory "ログエントリの生成とランダムメッセージ選択"
    note for FileUploadService "ファイルアップロードのユースケース担当、リトライ制御"
    note for IoTHubClient "Azure IoT Hubとの直接通信 (SAS URI取得、アップロード通知)"
    note for DialogService "各種ダイアログ表示"
    note for AppSettings "appsettings.jsonからの設定読み込み"
    note for AuthenticationInfo "認証情報"
```

**説明:**

*   主要なクラスとその責務、関連を示しています。
*   `LogFileHandler` はログファイルの操作を担当し、メモリ上に全ログエントリを保持しません。
*   イベントクラスは、処理結果やドメインイベントを運ぶデータ構造として概念的に示しています。

## チェックリスト

（静的モデリングルールにチェックリストのテンプレートがないため、ここでは省略します。）

## 補足

イベントストーミングステップ4の保留事項について、以下の通り仕様を決定しました。

1.  **ログのランダムメッセージ生成ロジック:**
    *   START: `運転を開始しました。`, `加工シーケンスを開始します。`
    *   STOP: `運転を停止しました。`, `現在の加工サイクルを完了し、停止しました。`
    *   WARN: `主軸モーターの温度が上昇しています。確認してください。`, `切削油の残量が少なくなっています。補充を検討してください。`
    *   ERROR: `サーボモーターエラーが発生しました。システムを停止します。 (コード: E012)`, `工具が破損しました。交換が必要です。機械を停止しました。`
2.  **IoT Hubへの再接続試行ロジック:** 1回のアップロード処理におけるIoT Hubへの再接続試行は最大3回、各試行の間隔は5秒とします。
3.  **UIブロッキングの具体的な実装方法:** アップロード処理中およびログ書き込み処理中は、メインウィンドウ全体を操作不可にし、マウスカーソルを待機状態にします。
4.  **アップロードタイムアウトの扱い:** Azure IoT Hub SDKまたは関連するHTTPクライアントのデフォルトタイムアウト設定に従います（最大5秒目安）。
5.  **LogFile集約とファイルシステム操作の整合性担保（ファイルI/Oエラー時）:** エラーダイアログを表示してユーザーに通知し、ログ書き込みは失敗とします。
6.  **LogManagementContextと外部システム間の具体的な連携方法:** イベントストーミングで定義されたコマンドやイベントの粒度でシーケンス図・クラス図を作成しました。
7.  **設定情報（認証情報）の安全な管理方法:** `appsettings.json` に埋め込み、一般的な方法（実行ファイルと同じ場所に配置）で管理します。

## 変更履歴

| 更新日時                         | 変更点   |
| :------------------------------- | :------- |
| 2025-05-11T23:01:00+09:00        | 新規作成 |
