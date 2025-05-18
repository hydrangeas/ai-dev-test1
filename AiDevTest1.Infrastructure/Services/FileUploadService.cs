using AiDevTest1.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace AiDevTest1.Infrastructure.Services
{
  public class FileUploadService : IFileUploadService
  {
    public Task UploadFileAsync(string filePath, string destinationPath)
    {
      // ダミー実装: コンソールに出力するだけ
      Console.WriteLine($"[FileUpload]: Uploading {filePath} to {destinationPath}");
      return Task.CompletedTask;
    }
  }
}
