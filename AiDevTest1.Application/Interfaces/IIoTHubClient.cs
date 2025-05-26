using System.Threading.Tasks;
using AiDevTest1.Application.Models;

namespace AiDevTest1.Application.Interfaces;

/// <summary>
/// Azure IoT Hubとの通信を担当するクライアントインターフェース
/// </summary>
public interface IIoTHubClient
{
  /// <summary>
  /// ファイルアップロード用のSAS URIを取得します
  /// </summary>
  /// <param name="blobName">アップロードするファイルのBlob名</param>
  /// <returns>SAS URIと相関IDを含む結果</returns>
  Task<SasUriResult> GetFileUploadSasUriAsync(string blobName);

  /// <summary>
  /// 指定されたSAS URIを使用してファイルをBlobストレージにアップロードします
  /// </summary>
  /// <param name="sasUri">SAS URI</param>
  /// <param name="fileContent">アップロードするファイルの内容</param>
  /// <returns>アップロード操作の結果</returns>
  Task<UploadToBlobResult> UploadToBlobAsync(string sasUri, byte[] fileContent);

  /// <summary>
  /// ファイルアップロード完了をIoT Hubに通知します
  /// </summary>
  /// <param name="correlationId">相関ID</param>
  /// <param name="isSuccess">アップロードが成功したかどうか</param>
  /// <returns>通知操作の結果</returns>
  Task<Result> NotifyFileUploadCompleteAsync(string correlationId, bool isSuccess);
}
