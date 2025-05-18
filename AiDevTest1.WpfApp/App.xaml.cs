using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AiDevTest1.WpfApp
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : System.Windows.Application
  {
    private IHost _host;

    public App()
    {
      _host = Host.CreateDefaultBuilder()
          .ConfigureServices((context, services) =>
          {
            ConfigureServices(services);
          })
          .Build();
    }

    private void ConfigureServices(IServiceCollection services)
    {
      // Register services here
      // e.g., services.AddSingleton<IMyService, MyService>();

      // Register Views and ViewModels
      services.AddSingleton<MainWindow>();
      // e.g., services.AddTransient<MyViewModel>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      await _host.StartAsync();

      var mainWindow = _host.Services.GetRequiredService<MainWindow>();
      mainWindow.Show();

      base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      using (_host)
      {
        await _host.StopAsync(TimeSpan.FromSeconds(5)); // Allow 5 seconds for graceful shutdown
      }

      base.OnExit(e);
    }
  }
}
