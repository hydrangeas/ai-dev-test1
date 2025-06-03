# 統合テスト (Integration Tests)

この統合テストスイートは、ログ管理システムの全体的なアーキテクチャが正しく動作することを検証します。

## テストクラス

### 1. LogManagementIntegrationTests
**目的**: 基本的な統合機能の検証
- コマンド → ハンドラー → イベント → イベントハンドラーの流れ
- DIコンテナの正しい設定
- サービス間の相互作用

**主要テストケース**:
- `WriteLogCommand_ShouldTriggerEventDispatch`: ログ書き込みコマンドがイベント配信を正しく実行
- `UploadFileCommand_Success_ShouldTriggerFileUploadedEvent`: 成功時のファイルアップロードフロー
- `UploadFileCommand_Failure_ShouldTriggerFileUploadFailedEvent`: 失敗時のエラーハンドリング
- `EventDispatcher_ShouldBeRegistered`: イベントディスパッチャーの登録確認
- `EventHandlers_ShouldBeRegistered`: 全イベントハンドラーの登録確認

### 2. LogManagementPerformanceTests
**目的**: システムのパフォーマンス特性の検証
- 大量データ処理能力
- 並行処理の安全性
- メモリリークの防止
- レスポンス時間の監視

**主要テストケース**:
- `WriteLogCommand_BulkExecution_ShouldCompleteWithinTimeLimit`: 100件のコマンドを5秒以内で処理
- `UploadFileCommand_ConcurrentExecution_ShouldHandleParallelRequests`: 10件の並行アップロード
- `EventDispatcher_HighVolumeEvents_ShouldMaintainPerformance`: 200件のイベントを2秒以内で配信
- `CommandHandler_MemoryUsage_ShouldNotCauseMemoryLeak`: メモリリークの検証
- `ServiceResolution_ShouldBeEfficient`: DIコンテナの解決効率

### 3. EndToEndScenarioTests
**目的**: 実際のアプリケーション使用パターンの検証
- ユーザーワークフローの模倣
- エラー回復シナリオ
- アプリケーションライフサイクル全体

**主要テストケース**:
- `TypicalUserWorkflow_WriteLogsAndUpload_ShouldCompleteSuccessfully`: 典型的なユーザー操作フロー
- `ErrorRecoveryScenario_UploadFailureAndRetry_ShouldHandleGracefully`: エラー回復とリトライ
- `HighFrequencyLogging_ShouldMaintainEventOrdering`: 高頻度ロギングでのイベント順序
- `CompleteApplicationLifecycle_ShouldWorkSeamlessly`: アプリケーション全体のライフサイクル

## テスト構成

### DIコンテナのセットアップ
各テストクラスは、実際のアプリケーションと同じDIコンテナ設定を使用します：

```csharp
// Core Services
services.AddTransient<ILogEntryFactory, LogEntryFactory>();
services.AddSingleton<ILogFileHandler, LogFileHandler>();

// Command Handlers  
services.AddTransient<ICommandHandler<WriteLogCommand>, WriteLogCommandHandler>();
services.AddTransient<ICommandHandler<UploadFileCommand>, UploadFileCommandHandler>();

// Event Infrastructure
services.AddSingleton<IEventDispatcher, EventDispatcher>();
services.AddTransient<IEventHandler<LogWrittenToFileEvent>, LogWrittenToFileEventHandler>();
// ... 他のイベントハンドラー
```

### モック戦略
外部依存関係はモックを使用：
- `IIoTHubClient`: Azure IoT Hubとの通信
- `ILogFileHandler`: ファイルシステムアクセス (基本テスト用)

実際のサービス実装を使用：
- `EventDispatcher`: イベント配信ロジック
- `LogEntryFactory`: ログエントリ生成
- すべてのイベントハンドラー

## テスト実行要件

### 前提条件
- .NET 8.0以降
- xUnit テストランナー
- FluentAssertions
- Moq

### パフォーマンス基準
- ログコマンド100件: 5秒以内
- 並行アップロード10件: 3秒以内  
- イベント配信200件: 2秒以内
- メモリ増加: 10MB以下
- サービス解決1000回: 100ms以内

### CI/CD統合
これらのテストはCI/CDパイプラインで自動実行され、以下を検証します：
- 機能回帰の防止
- パフォーマンス劣化の検出
- アーキテクチャ整合性の確認

## 特殊なテストヘルパー

### EventCapture
イベントの発火と順序を追跡するためのヘルパークラス。
エンドツーエンドテストで使用され、イベントフローの正確性を検証します。

### TrackingEventDispatcher
実際のEventDispatcherをラップし、イベント追跡機能を追加。
テスト中のイベント配信を監視して、期待される動作を確認します。

## テスト戦略

### 統合レベル
- **Unit Tests**: 個別コンポーネント (別ディレクトリ)
- **Integration Tests**: コンポーネント間相互作用 (このディレクトリ)
- **End-to-End Tests**: 完全なアプリケーションフロー (このディレクトリ)

### テストデータ管理
- 一時ディレクトリの使用
- テスト完了後の自動クリーンアップ
- 分離されたテスト環境

### エラーシミュレーション
- ネットワーク障害
- ファイルシステムエラー
- Azure IoT Hub接続問題
- リソース不足状況

これらの統合テストにより、リファクタリング後のアーキテクチャが正しく機能し、
実際の運用環境での要求を満たすことを確認します。