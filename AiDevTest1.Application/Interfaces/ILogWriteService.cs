using AiDevTest1.Application.Models;

namespace AiDevTest1.Application.Interfaces
{
  /// <summary>
  /// ログ書き込みサービスのインターフェース
  /// </summary>
  public interface ILogWriteService
  {
    /// <summary>
    /// ランダムなログエントリを生成してファイルに非同期で書き込みます
    /// </summary>
    /// <returns>操作の成功/失敗を表すResult</returns>
    Task<Result> WriteLogEntryAsync();
  }
}
