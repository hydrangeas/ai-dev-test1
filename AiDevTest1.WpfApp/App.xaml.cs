using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AiDevTest1.Application.Interfaces; // Added for services
using AiDevTest1.Application.Models;    // Added for AuthenticationInfo
using AiDevTest1.Infrastructure.Services; // Added for services
using AiDevTest1.WpfApp.ViewModels;     // Added for MainWindowViewModel
using Microsoft.Extensions.Logging;
using System;
using System.IO; // Added for File logging
using System.Windows;

namespace AiDevTest1.WpfApp
{
  public partial class App : System.Windows.Application
  {
    private const string LogFilePath = "debug_log.txt";
    private static void Log(string message)
    {
      try { File.AppendAllText(LogFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}"); } catch { /* ignore logging errors */ }
    }

    public static IHost? AppHost { get; private set; }

    public App()
    {
      Log("[APP CONSTRUCTOR] Entered App constructor (Phase 2: Full Init with File Logging).");
      try
      {
        AppHost = Host.CreateDefaultBuilder() // Removed args
                                              // .ConfigureHostConfiguration(configHost => // Moved SetBasePath to ConfigureAppConfiguration
                                              // {
                                              //   Log($"[APP CONSTRUCTOR] Setting base path for configuration to: {AppContext.BaseDirectory}");
                                              //   configHost.SetBasePath(AppContext.BaseDirectory);
                                              // })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
              Log($"[APP CONSTRUCTOR] Setting base path for app configuration to: {AppContext.BaseDirectory}");
              config.SetBasePath(AppContext.BaseDirectory); // Set base path here
              config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
              config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
              Log($"[APP CONSTRUCTOR] Attempting to load appsettings for environment: {hostingContext.HostingEnvironment.EnvironmentName}");
            })
            .ConfigureServices((hostContext, services) =>
            {
              ConfigureServices(services, hostContext.Configuration);
            })
            .Build();
        Log("[APP CONSTRUCTOR] Host built successfully with services.");
      }
      catch (Exception ex)
      {
        Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Log("[APP CONSTRUCTOR ERROR - FULL INIT]");
        Log($"Message: {ex.Message}");
        Log($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
          Log($"Inner Exception Message: {ex.InnerException.Message}");
          Log($"Inner Exception StackTrace: {ex.InnerException.StackTrace}");
        }
        Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Environment.Exit(-1);
      }
      Log("[APP CONSTRUCTOR] Exiting constructor (Phase 2: Full Init with File Logging).");
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
      Log("[CONFIGURE_SERVICES] Entered ConfigureServices.");
      var authSection = configuration.GetSection("AuthInfo");
      var connectionStringFromConfig = authSection["ConnectionString"];
      Log($"[CONFIGURE_SERVICES] AuthInfo section exists: {authSection.Exists()}");
      Log($"[CONFIGURE_SERVICES] ConnectionString from IConfiguration: '{connectionStringFromConfig}'");

      services.Configure<AuthenticationInfo>(authSection);
      Log("[CONFIGURE_SERVICES] AuthenticationInfo configured.");

      // Services
      services.AddSingleton<ILogWriteService, LogWriteService>();
      services.AddTransient<IFileUploadService, FileUploadService>();
      Log("[CONFIGURE_SERVICES] App Services configured.");

      // ViewModels
      services.AddTransient<MainWindowViewModel>();
      Log("[CONFIGURE_SERVICES] ViewModels configured.");

      // Main Window
      services.AddSingleton<MainWindow>();
      Log("[CONFIGURE_SERVICES] MainWindow configured.");
      Log("[CONFIGURE_SERVICES] Exiting ConfigureServices (All services restored).");
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      Log("[APP ONSTARTUP] Entered OnStartup (Full App Run).");
      try
      {
        if (AppHost == null)
        {
          Log("[APP ONSTARTUP] AppHost is null. Cannot start application.");
          Environment.Exit(-1);
          return;
        }
        await AppHost.StartAsync();
        Log("[APP ONSTARTUP] AppHost started successfully.");

        var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
        Log("[APP ONSTARTUP] MainWindow resolved from DI.");
        mainWindow.Show();
        Log("[APP ONSTARTUP] mainWindow.Show() called.");

        base.OnStartup(e);
      }
      catch (Exception ex)
      {
        Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Log("[APP ONSTARTUP ERROR - APP RUN]");
        Log($"Message: {ex.Message}");
        Log($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
          Log($"Inner Exception Message: {ex.InnerException.Message}");
          Log($"Inner Exception StackTrace: {ex.InnerException.StackTrace}");
        }
        Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        Environment.Exit(-1);
      }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
      Log("[APP ONEXIT] Entered OnExit.");
      if (AppHost != null)
      {
        try
        {
          await AppHost.StopAsync();
          Log("[APP ONEXIT] AppHost stopped.");
          AppHost.Dispose();
          Log("[APP ONEXIT] AppHost disposed.");
        }
        catch (Exception ex)
        {
          Log($"[APP ONEXIT] Error during AppHost stop/dispose: {ex.Message}");
        }
      }
      base.OnExit(e);
    }
  } // Closing brace for class App
} // Closing brace for namespace AiDevTest1.WpfApp
