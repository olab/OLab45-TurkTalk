using Microsoft.AspNetCore.Authentication.JwtBearer;
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
using OLab.TurkTalk.Service.Azure.Interfaces;
using OLab.TurkTalk.Service.Azure.Middleware;
using OLab.TurkTalk.Service.Azure.Services;
using System;
using System.Configuration;
using System.Threading.Tasks;

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
#if DEBUG
        Formatting = Formatting.Indented,
#endif
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

      services.AddScoped<IUserContext, UserContext>();
      services.AddSingleton<IOLabLogger, OLabLogger>();
      services.AddSingleton<IOLabConfiguration, OLabConfiguration>();
      services.AddSingleton<IUserService, UserService>();

      services.AddSingleton<IConference>((s) =>
      {
        return new Conference(
          s.GetRequiredService<IOLabLogger>(),
          s.GetRequiredService<IOLabConfiguration>(),
          s.GetRequiredService<OLabDBContext>(),
          "default");
      });

    })

    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OLabAuthMiddleware>();
      builder.Services
        .AddSingleton<SignalRService>()
        .AddHostedService(sp => sp.GetRequiredService<SignalRService>())
        .AddSingleton<IHubContextStore>(sp => sp.GetRequiredService<SignalRService>());
    })
    .Build();

host.Run();
