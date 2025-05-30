using System;
using System.Threading;
using System.Threading.Tasks;

namespace AiDevTest1.Application.Interfaces
{
  /// <summary>
  /// リトライポリシーのインターフェース
  /// 失敗した操作を指定された戦略に従って再実行する機能を提供します
  /// </summary>
  public interface IRetryPolicy
  {
    /// <summary>
    /// 指定された操作をリトライポリシーに従って実行します
    /// </summary>
    /// <typeparam name="T">操作の戻り値の型</typeparam>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>操作の実行結果</returns>
    /// <exception cref="ArgumentNullException">operationがnullの場合</exception>
    Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定された操作をリトライポリシーに従って実行します（戻り値なし）
    /// </summary>
    /// <param name="operation">実行する非同期操作</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <exception cref="ArgumentNullException">operationがnullの場合</exception>
    Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default);
  }
}
