using System;
using System.Runtime.Serialization;

namespace AiDevTest1.Domain.Exceptions
{
  /// <summary>
  /// ドメイン層で発生する例外の基底クラス。
  /// ビジネスルール違反やドメイン固有のエラーを表現します。
  /// </summary>
  [Serializable]
  public class DomainException : Exception
  {
    /// <summary>
    /// デフォルトのエラーカテゴリ。
    /// </summary>
    private const string DefaultCategory = "Domain";

    /// <summary>
    /// エラーコード。エラーの種類を識別するために使用します。
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;

    /// <summary>
    /// エラーカテゴリ。エラーを分類するために使用します。
    /// </summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    public DomainException(string message)
        : this(message, null, null, null)
    {
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    public DomainException(string message, string? errorCode)
        : this(message, errorCode, null, null)
    {
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="category">エラーカテゴリ。</param>
    public DomainException(string message, string? errorCode, string? category)
        : base(message)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = category ?? DefaultCategory;
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, Exception? innerException)
        : this(message, null, null, innerException)
    {
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, string? errorCode, Exception? innerException)
        : this(message, errorCode, null, innerException)
    {
    }

    /// <summary>
    /// <see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="message">例外を説明するメッセージ。</param>
    /// <param name="errorCode">エラーコード。</param>
    /// <param name="category">エラーカテゴリ。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public DomainException(string message, string? errorCode, string? category, Exception? innerException)
        : base(message, innerException)
    {
      ErrorCode = errorCode ?? GetType().Name.Replace("Exception", string.Empty);
      Category = category ?? DefaultCategory;
    }

    /// <summary>
    /// シリアル化されたデータから<see cref="DomainException"/>クラスの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="info">シリアル化されたオブジェクトデータを保持するSerializationInfo。</param>
    /// <param name="context">転送元または転送先に関するコンテキスト情報を含むStreamingContext。</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    protected DomainException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
      ErrorCode = info.GetString(nameof(ErrorCode)) ?? string.Empty;
      Category = info.GetString(nameof(Category)) ?? string.Empty;
    }

    /// <summary>
    /// 例外に関する情報をSerializationInfoに設定します。
    /// </summary>
    /// <param name="info">シリアル化されたオブジェクトデータを保持するSerializationInfo。</param>
    /// <param name="context">転送元または転送先に関するコンテキスト情報を含むStreamingContext。</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null) throw new ArgumentNullException(nameof(info));

      info.AddValue(nameof(ErrorCode), ErrorCode);
      info.AddValue(nameof(Category), Category);
      base.GetObjectData(info, context);
    }
  }
}
