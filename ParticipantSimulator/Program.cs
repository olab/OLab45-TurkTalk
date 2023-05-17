using System;
using System.Security.Cryptography.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog;
using OLab.TurkTalk.ParticipantSimulator.SimulationThread;

namespace OLab.TurkTalk.ParticipantSimulator
{
  internal class Program
  {
    public static IConfiguration Configuration { get; set; }
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    static void Main(string[] args)
    {
      Logger.Info("Application started...");

      Configuration = new ConfigurationBuilder()
#if DEBUG
          .AddJsonFile($"appsettings.Development.json", true, true)
#else
          .AddJsonFile("appsettings.json", true, true)
#endif
          .AddEnvironmentVariables()
          .Build();

      if (Configuration == null)
        return;

      var settings = Configuration.GetRequiredSection("Settings").Get<Settings>();
      if (settings == null)
        throw new ArgumentException("Could not load settings");

      if (settings.LogDirectory == null)
        LogManager.Configuration.Variables["logdirectory"]
          = System.Reflection.Assembly.GetExecutingAssembly().Location;
      else
        LogManager.Configuration.Variables["logdirectory"]
          = settings.LogDirectory;

      var worker = new SimulatorWorker(settings, Logger);
      worker.ExecuteWorkers();

      Logger.Info("Application Ended!");

    }
  }

}