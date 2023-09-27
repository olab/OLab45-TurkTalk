using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Service.Azure.BusinessObjects;
using System;

namespace OLab.TurkTalk.Service.Azure;

public class Program
{
  public static IConfiguration Configuration { get; }

  public static void Main()
  {
    var host = new HostBuilder()
      .ConfigureAppConfiguration(builder =>
      {
        builder.AddJsonFile(
          "local.settings.json",
          optional: true,
          reloadOnChange: true);
      })
      .ConfigureServices((context, services) =>
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented,
          ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var connectionString = Environment.GetEnvironmentVariable("DefaultDatabase");
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<OLabDBContext>(options =>
          options.UseMySql(connectionString, serverVersion)
            .EnableDetailedErrors());
        //.AddLogging(options => options.SetMinimumLevel(LogLevel.Information));

        services.AddOptions<AppSettings>()
          .Configure<IConfiguration>((options, c) =>
          {
            c.GetSection("AppSettings").Bind(options);
          });

        services.AddSingleton<IOLabLogger, OLabLogger>();
        services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
        services.AddSingleton<Conference>();
      })
      .ConfigureFunctionsWorkerDefaults()
      .Build();

    host.Run();
  }
}