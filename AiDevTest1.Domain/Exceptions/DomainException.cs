using System;

namespace AiDevTest1.Domain.Exceptions
{
  /// <summary>
  /// ドメイン層で発生する例外の基底クラス。
  /// ビジネスルール違反やドメイン固有のエラーを表現します。
  /// </summary>
  public class DomainException : Exception
  {
    /// <summary>
    /// エラーコード。エラーの種類を識別するために使用します。
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// エラーカテゴリ。エラーを分類するために使用します。
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    public DomainException(string message)
        : base(message)
    {
      ErrorCode = GetType().Name.Replace("Exception", string.Empty);
      Category = "Domain";
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    public DomainException(string message, string errorCode)
        : base(message)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = "Domain";
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="category">エラーカテゴリ。</param>
    public DomainException(string message, string errorCode, string category)
        : base(message)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = category ?? "Domain";
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
      ErrorCode = GetType().Name.Replace("Exception", string.Empty);
      Category = "Domain";
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = "Domain";
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="category">エラーカテゴリ。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, string errorCode, string category, Exception innerException)
        : base(message, innerException)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = category ?? "Domain";
    }
  }
}
