using System.Threading.Tasks;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Application.Interfaces
{
  public interface IFileUploadService
  {
    /// <summary>
    /// ログファイルをIoT Hub経由でBlobストレージにアップロードします
    /// </summary>
    /// <returns>アップロード操作の結果</returns>
    Task<Result> UploadLogFileAsync();
  }
}
