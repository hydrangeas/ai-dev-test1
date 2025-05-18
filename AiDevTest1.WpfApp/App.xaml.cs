using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Infrastructure.Services;
using AiDevTest1.WpfApp.ViewModels;
using System.Windows;

namespace AiDevTest1.WpfApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : System.Windows.Application
  {
    public static IHost? AppHost { get; private set; }

    public App()
    {
      AppHost = Host.CreateDefaultBuilder()
          .ConfigureServices((hostContext, services) =>
          {
            ConfigureServices(services);
          })
          .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
      // Services
      services.AddSingleton<ILogWriteService, LogWriteService>();
      services.AddTransient<IFileUploadService, FileUploadService>();

      // ViewModels
      services.AddTransient<MainWindowViewModel>();

      // Main Window
      services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      ArgumentNullException.ThrowIfNull(AppHost);
      await AppHost.StartAsync();

      var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
      mainWindow.Show();

      base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      if (AppHost != null)
      {
        await AppHost.StopAsync();
        AppHost.Dispose();
      }
      base.OnExit(e);
    }
  }
}
