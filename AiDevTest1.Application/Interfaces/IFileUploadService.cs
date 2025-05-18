using System.Threading.Tasks;

namespace AiDevTest1.Application.Interfaces
{
  public interface IFileUploadService
  {
    Task UploadFileAsync(string filePath, string destinationPath);
  }
}
