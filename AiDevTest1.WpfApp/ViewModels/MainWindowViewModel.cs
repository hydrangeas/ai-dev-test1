using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AiDevTest1.Application.Interfaces;
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
    private readonly ILogWriteService _logWriteService;
    private readonly IFileUploadService _fileUploadService;

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
    /// <param name="logWriteService">ログ書き込みサービス</param>
    /// <param name="fileUploadService">ファイルアップロードサービス</param>
    public MainWindowViewModel(ILogWriteService logWriteService, IFileUploadService fileUploadService)
    {
      _logWriteService = logWriteService ?? throw new ArgumentNullException(nameof(logWriteService));
      _fileUploadService = fileUploadService ?? throw new ArgumentNullException(nameof(fileUploadService));

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

        // ログエントリの書き込み実行
        var result = await _logWriteService.WriteLogEntryAsync();

        // NOTE: issue #18では IDialogService のDI注入が仕様とされていたが、
        // issue #13のDialogService実装が未完了のため、暫定的に既存のDialogHelperを直接使用
        // 将来的にはIDialogServiceインターフェースとDI注入に移行することを想定

        if (result.IsSuccess)
        {
          // ログ書き込み成功時のみファイルアップロードを実行
          var uploadResult = await _fileUploadService.UploadLogFileAsync();
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
  }
}