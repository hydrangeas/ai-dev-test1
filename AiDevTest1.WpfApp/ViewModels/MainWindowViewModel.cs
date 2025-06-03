using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Commands;
using AiDevTest1.WpfApp.Helpers;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AiDevTest1.WpfApp.ViewModels
{
  /// <summary>
  /// MainWindowのViewModelクラス（MVVMパターン）
  /// </summary>
  public partial class MainWindowViewModel : ObservableObject
  {
    private readonly ICommandHandler<WriteLogCommand> _writeLogCommandHandler;
    private readonly ICommandHandler<UploadFileCommand> _uploadFileCommandHandler;

    /// <summary>
    /// 処理実行中かどうかを示すフラグ
    /// </summary>
    [ObservableProperty]
    private bool _isProcessing;

    /// <summary>
    /// ログ書き込みコマンド
    /// </summary>
    public IAsyncRelayCommand LogWriteCommand { get; }

    /// <summary>
    /// コンストラクタ（依存性注入）
    /// </summary>
    /// <param name="writeLogCommandHandler">ログ書き込みコマンドハンドラー</param>
    /// <param name="uploadFileCommandHandler">ファイルアップロードコマンドハンドラー</param>
    public MainWindowViewModel(ICommandHandler<WriteLogCommand> writeLogCommandHandler, ICommandHandler<UploadFileCommand> uploadFileCommandHandler)
    {
      _writeLogCommandHandler = writeLogCommandHandler ?? throw new ArgumentNullException(nameof(writeLogCommandHandler));
      _uploadFileCommandHandler = uploadFileCommandHandler ?? throw new ArgumentNullException(nameof(uploadFileCommandHandler));

      // ログ書き込みコマンドの初期化
      LogWriteCommand = new AsyncRelayCommand(ExecuteLogWriteAsync, CanExecuteLogWrite);
    }

    /// <summary>
    /// ログ書き込みコマンドの実行可否を判定します
    /// </summary>
    /// <returns>実行可能な場合はtrue、そうでなければfalse</returns>
    private bool CanExecuteLogWrite()
    {
      // 処理中でない場合のみ実行可能
      return !IsProcessing;
    }

    /// <summary>
    /// ログ書き込みコマンドの実行処理
    /// </summary>
    /// <returns>非同期タスク</returns>
    private async Task ExecuteLogWriteAsync()
    {
      try
      {
        // UIブロッキング開始
        IsProcessing = true;

        // コマンドの実行可否状態を更新
        LogWriteCommand.NotifyCanExecuteChanged();

        // コマンドパターンを使用してログエントリの書き込みを実行
        var command = new WriteLogCommand();
        var result = await _writeLogCommandHandler.HandleAsync(command);

        // NOTE: issue #18では IDialogService のDI注入が仕様とされていたが、
        // issue #13のDialogService実装が未完了のため、暫定的に既存のDialogHelperを直接使用
        // 将来的にはIDialogServiceインターフェースとDI注入に移行することを想定

        if (result.IsSuccess)
        {
          // ログ書き込み成功時のみファイルアップロードを実行
          var uploadCommand = new UploadFileCommand();
          var uploadResult = await _uploadFileCommandHandler.HandleAsync(uploadCommand);
          if (uploadResult.IsSuccess)
          {
            // 全処理成功
            DialogHelper.ShowSuccess("処理が完了しました。");
          }
          else
          {
            // ファイルアップロード失敗
            DialogHelper.ShowFailure($"ファイルアップロードに失敗しました: {uploadResult.ErrorMessage}");
          }
        }
        else
        {
          // ログ書き込み失敗
          DialogHelper.ShowFailure($"ログの書き込みに失敗しました: {result.ErrorMessage}");
          // ログ書き込み失敗時はファイルアップロードをスキップ
        }
      }
      catch (Exception ex)
      {
        // 予期しないエラーのハンドリング
        DialogHelper.ShowFailure($"予期しないエラーが発生しました: {ex.Message}");
      }
      finally
      {
        // UIブロッキング解除
        IsProcessing = false;

        // コマンドの実行可否状態を更新
        LogWriteCommand.NotifyCanExecuteChanged();
      }
    }

    /// <summary>
    /// ウィンドウが閉じられる際の処理を行います
    /// </summary>
    /// <returns>ウィンドウを閉じることができる場合はtrue、そうでなければfalse</returns>
    public bool OnWindowClosing()
    {
      // 処理中の場合は終了をブロック
      if (IsProcessing)
      {
        DialogHelper.ShowWarning("処理中のため、終了できません。処理完了後に再度お試しください。");
        return false;
      }

      // リソースクリーンアップ処理
      CleanupResources();
      return true;
    }

    /// <summary>
    /// リソースのクリーンアップ処理を行います
    /// </summary>
    private void CleanupResources()
    {
      // 現状では特に複雑なリソース管理はないため最小限の実装
      // 将来的に必要なリソース解放処理をここに追加

      // 例: イベントハンドラーの解除、IDisposableリソースの破棄など
      // _someDisposableResource?.Dispose();
    }
  }
}
