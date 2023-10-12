using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OLab.Api.Data.Interface;
using OLab.Api.Model;
using OLab.Api.Utils;
using OLab.Common.Interfaces;
using OLab.Common.Utils;
using OLab.TurkTalk.Service.Azure;
using OLab.TurkTalk.Service.Azure.BusinessObjects;
using OLab.TurkTalk.Service.Azure.Services;
using System;

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

      services.AddCors(options =>
      {
        options.AddPolicy("CorsPolicy",
           builder => builder
            // .AllowAnyOrigin()
            .WithOrigins("http://localhost:4000", "http://localhost:3000", "https://cloud.olab.ca")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
          );
      });

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

      services.AddTransient<IUserContext, UserContext>();
      services.AddSingleton<IOLabLogger, OLabLogger>();
      services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
      services.AddSingleton<Conference>();
    })

    .ConfigureFunctionsWorkerDefaults(b => b.Services
                .AddSingleton<SignalRService>()
                .AddHostedService(sp => sp.GetRequiredService<SignalRService>())
                .AddSingleton<IHubContextStore>(sp => sp.GetRequiredService<SignalRService>()))
    .Build();

host.Run();
