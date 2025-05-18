using AiDevTest1.Application.Models;
using Microsoft.Extensions.Options;

namespace AiDevTest1.WpfApp.ViewModels
{
  public class MainWindowViewModel
  {
    public string ConnectionStringForDisplay { get; private set; }

    public MainWindowViewModel(IOptions<AuthenticationInfo>? authInfoOptions) // Made authInfoOptions nullable
    {
      if (authInfoOptions != null && authInfoOptions.Value != null)
      {
        ConnectionStringForDisplay = authInfoOptions.Value.ConnectionString;
      }
      else
      {
        ConnectionStringForDisplay = "AuthenticationInfo not configured or ConnectionString is null.";
      }
    }
  }
}
