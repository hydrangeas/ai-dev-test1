using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AiDevTest1.Application.Interfaces;
using AiDevTest1.Application.Commands;
using AiDevTest1.Application.Handlers;
using AiDevTest1.Domain.Interfaces;
using AiDevTest1.Domain.Services;
using AiDevTest1.Infrastructure.Configuration;
using AiDevTest1.Infrastructure.Policies;
using AiDevTest1.Infrastructure.Services;
using AiDevTest1.Infrastructure.Events;
using AiDevTest1.WpfApp.ViewModels;
using AiDevTest1.WpfApp.Services;
using AiDevTest1.Application.EventHandlers;
using AiDevTest1.Domain.Events;
using Microsoft.Extensions.Logging; // Kept for ILogger if used by Host.CreateDefaultBuilder or future use
using System;
using System.Windows;

namespace AiDevTest1.WpfApp
{
  public partial class App : System.Windows.Application
  {
    public static IHost? AppHost { get; private set; }

    public App()
    {
      try
      {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
              config.SetBasePath(AppContext.BaseDirectory);
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
              ConfigureServices(services, hostContext.Configuration);
            })
            .Build();
      }
      catch (Exception ex)
      {
        // Consider a more robust way to display/log critical startup errors
        // For now, exiting is a simple approach.
        MessageBox.Show($"Critical application startup error: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(-1);
      }
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
      var authSection = configuration.GetSection("AuthInfo");
      services.Configure<IoTHubConfiguration>(authSection);

      // Factories and Handlers
      services.AddTransient<ILogEntryFactory, LogEntryFactory>();
      services.AddTransient<ILogFileHandler, LogFileHandler>();

      // Command Handlers
      services.AddTransient<ICommandHandler<WriteLogCommand>, WriteLogCommandHandler>();
      services.AddTransient<ICommandHandler<UploadFileCommand>, UploadFileCommandHandler>();

      // Policies
      services.AddTransient<IRetryPolicy, ExponentialBackoffRetryPolicy>();
      services.AddTransient<IDialogDisplayPolicy, StandardDialogDisplayPolicy>();

      // Services
      services.AddSingleton<ILogWriteService, LogWriteService>();
      services.AddTransient<IFileUploadService, FileUploadService>();
      services.AddSingleton<IIoTHubClient, IoTHubClient>();

      // Event Dispatcher
      services.AddSingleton<IEventDispatcher, EventDispatcher>();

      // Event Handlers
      services.AddTransient<IEventHandler<LogWrittenToFileEvent>, LogWrittenToFileEventHandler>();
      services.AddTransient<IEventHandler<FileUploadedEvent>, FileUploadedEventHandler>();
      services.AddTransient<IEventHandler<FileUploadFailedEvent>, FileUploadFailedEventHandler>();

      // UI Services
      services.AddSingleton<DialogService>();
      services.AddSingleton<IDialogService>(provider =>
      {
        var dialogService = provider.GetRequiredService<DialogService>();
        var displayPolicy = provider.GetRequiredService<IDialogDisplayPolicy>();
        return new PolicyBasedDialogService(dialogService, displayPolicy);
      });

      // ViewModels
      services.AddSingleton<MainWindowViewModel>();

      // Main Window
      services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      try
      {
        if (AppHost == null)
        {
          MessageBox.Show("Application host is not initialized.", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
          Environment.Exit(-1);
          return;
        }
        await AppHost.StartAsync();

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
      }
      catch (Exception ex)
      {
        // Consider a more robust way to display/log critical startup errors
        MessageBox.Show($"Error during application startup: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(-1);
      }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      if (AppHost != null)
      {
        try
        {
          await AppHost.StopAsync();
          AppHost.Dispose();
        }
        catch (Exception ex)
        {
          // Log or handle error during host stop/dispose if necessary
          System.Diagnostics.Debug.WriteLine($"Error during AppHost stop/dispose: {ex.Message}");
        }
      }
      base.OnExit(e);
    }
  }
}
