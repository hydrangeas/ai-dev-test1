using AiDevTest1.Application.Interfaces;

namespace AiDevTest1.Application.Commands
{
    /// <summary>
    /// ログエントリの書き込みを指示するコマンド
    /// </summary>
    public class WriteLogCommand : ICommand
    {
        // 現時点では追加のプロパティは不要
        // 将来的にはEventTypeやメッセージなどを受け取る可能性あり
    }
}