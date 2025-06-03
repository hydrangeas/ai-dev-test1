# リファクタリング Issues #66-#76 完全分析

## 実装済みコンポーネント（#66-#74）

### ドメインイベント基盤（#66-#67）
- **IDomainEvent**: イベントの基本インターフェース
- **DomainEventBase**: 抽象基底クラス
- **具体的イベント**:
  - `LogWrittenToFileEvent`: ログ書き込み時に発生
  - `FileUploadedEvent`: ファイルアップロード成功時
  - `FileUploadFailedEvent`: ファイルアップロード失敗時

### イベントディスパッチャー（#68）
- **IEventDispatcher**: イベント配信インターフェース
- **EventDispatcher**: リフレクションベースの動的ハンドラー解決
- **IEventHandler<TEvent>**: イベントハンドラーインターフェース

### コマンドパターン（#69-#71）
- **ICommand**: コマンドマーカーインターフェース
- **ICommandHandler<TCommand>**: コマンドハンドラーインターフェース
- **実装済みコマンド**:
  - `WriteLogCommand` / `WriteLogCommandHandler`
  - `UploadFileCommand` / `UploadFileCommandHandler`

### ドメイン集約（#72）
- **LogFile**: ログファイル集約
  - ログエントリの管理
  - ドメインイベントの発生（`_domainEvents.Add`）
  - ビジネスルールの実装

### UIサービス（#73-#74）
- **IDialogService**: ダイアログ表示インターフェース
- **DialogService**: 基本実装
- **IDialogDisplayPolicy**: 表示ポリシーインターフェース
- **PolicyBasedDialogService**: ポリシーベースの実装

## 現在の問題点と未実装部分

### 1. イベントが発生しても配信されない
```csharp
// LogFile.cs
public void AddEntry(LogEntry entry)
{
    _entries.Add(entry);
    _domainEvents.Add(new LogWrittenToFileEvent(_filePath, entry)); // イベント発生
    // しかし、これらのイベントは配信されない！
}
```

### 2. EventDispatcherが未使用
- DIコンテナに登録済み
- しかし、どこからも呼び出されていない
- LogFile集約のドメインイベントが蓄積されるだけ

### 3. イベントハンドラーが存在しない
- `IEventHandler<TEvent>` インターフェースは定義済み
- しかし、具体的な実装クラスが1つもない

## Issue #75: イベントハンドラーの実装

### 実装すべきイベントハンドラー

#### 1. LogWrittenToFileEventHandler
```csharp
public class LogWrittenToFileEventHandler : IEventHandler<LogWrittenToFileEvent>
{
    private readonly IDialogService _dialogService;
    
    public async Task HandleAsync(LogWrittenToFileEvent domainEvent, CancellationToken cancellationToken)
    {
        // ログ書き込み成功の通知
        // 監査ログの記録
        // メトリクスの更新など
    }
}
```

#### 2. FileUploadedEventHandler
```csharp
public class FileUploadedEventHandler : IEventHandler<FileUploadedEvent>
{
    public async Task HandleAsync(FileUploadedEvent domainEvent, CancellationToken cancellationToken)
    {
        // アップロード成功の記録
        // 次回アップロード時刻の更新
        // 成功カウンターの更新
    }
}
```

#### 3. FileUploadFailedEventHandler
```csharp
public class FileUploadFailedEventHandler : IEventHandler<FileUploadFailedEvent>
{
    private readonly IRetryPolicy _retryPolicy;
    
    public async Task HandleAsync(FileUploadFailedEvent domainEvent, CancellationToken cancellationToken)
    {
        // エラーログの記録
        // リトライスケジュールの設定
        // アラート通知の検討
    }
}
```

### イベント配信の統合

コマンドハンドラーを更新してイベントを配信：

```csharp
public class WriteLogCommandHandler : ICommandHandler<WriteLogCommand>
{
    private readonly ILogWriteService _logWriteService;
    private readonly IEventDispatcher _eventDispatcher;
    
    public async Task<Result> HandleAsync(WriteLogCommand command, CancellationToken cancellationToken)
    {
        var result = await _logWriteService.WriteLogEntryAsync();
        
        if (result.IsSuccess)
        {
            // LogFileからドメインイベントを取得して配信
            // ※ LogWriteServiceの実装も確認が必要
        }
        
        return result;
    }
}
```

## Issue #76: アーキテクチャの完成

### 可能性1: クエリパターンの実装
- `IQuery<TResult>` インターフェース
- `IQueryHandler<TQuery, TResult>` インターフェース
- ログ履歴照会などの読み取り専用操作

### 可能性2: プロセスマネージャー/サガ
- 複数のコマンドを調整
- ログ書き込み→ファイルアップロードの一連の流れを管理

### 可能性3: 統合テストとサンプルシナリオ
- 完全なイベント駆動フローのデモンストレーション
- すべてのコンポーネントの統合

## 実装の優先順位

1. **Issue #75 - イベントハンドラー**
   - 各ドメインイベントのハンドラー実装
   - EventDispatcherの統合
   - コマンドハンドラーからのイベント配信

2. **Issue #76 - アーキテクチャ完成**
   - クエリパターンまたはプロセスマネージャー
   - 完全な統合テスト
   - ドキュメント更新

## アーキテクチャの利点

1. **疎結合**: コマンド、イベント、ハンドラーが独立
2. **拡張性**: 新しいハンドラーの追加が容易
3. **テスタビリティ**: 各コンポーネントが独立してテスト可能
4. **監査性**: イベントによる操作履歴の追跡

## 注意点

- LogFile集約で発生したイベントをどこで取得するか
- イベントの永続化は必要か（イベントソーシング）
- 非同期処理のエラーハンドリング
- イベントハンドラーの実行順序

## 重要な発見

現在の実装では、`LogWriteService` は `ILogFileHandler` を使用していますが、LogFile集約を直接使用していません。
そのため、ドメインイベントが発生していない可能性があります。

### 現在の実装フロー
1. `WriteLogCommandHandler` → `ILogWriteService`
2. `LogWriteService` → `ILogFileHandler`
3. `LogFileHandler` → ファイルに直接書き込み（LogFile集約を使用せず）

### 理想的な実装フロー
1. `WriteLogCommandHandler` → `ILogWriteService`
2. `LogWriteService` → LogFile集約を使用
3. LogFile集約 → ドメインイベント発生
4. `WriteLogCommandHandler` → `IEventDispatcher` でイベント配信
5. 各イベントハンドラーが処理

## Issue #75 の実装方針

1. **イベントハンドラーの実装**
   - 基本的なロギングとモニタリング機能
   - 将来の拡張を考慮した設計

2. **既存サービスの改修は最小限に**
   - 大規模な変更は避ける
   - 段階的な移行を考慮

3. **実用的なアプローチ**
   - 完璧なイベントソーシングではなく、実用的なイベント通知
   - 既存の動作を壊さない範囲での実装