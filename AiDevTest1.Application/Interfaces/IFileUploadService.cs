using System.Threading.Tasks;

namespace AiDevTest1.Application.Interfaces
{
  public interface IFileUploadService
  {
    Task UploadFileAsync(string filePath, string destinationPath);

    // TODO: #15 実装時に以下のメソッドを追加予定
    // Task<Result> UploadLogFileAsync();
    // このメソッドは:
    // - LogFileHandlerからファイルパスを取得
    // - IIoTHubClientを使用してSAS URI取得とアップロード実行
    // - #16でリトライロジック(最大3回、間隔5秒)を追加
    // - Result型で成功/失敗を返す
  }
}
