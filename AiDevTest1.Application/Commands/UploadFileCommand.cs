using AiDevTest1.Application.Interfaces;

namespace AiDevTest1.Application.Commands
{
    /// <summary>
    /// ファイルアップロードを指示するコマンド
    /// </summary>
    public class UploadFileCommand : ICommand
    {
        // 現時点では追加のプロパティは不要
        // 将来的にはファイルパスや対象ファイルの指定などを受け取る可能性あり
    }
}