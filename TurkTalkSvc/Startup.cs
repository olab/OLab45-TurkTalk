using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OLabWebAPI.Data;
using OLabWebAPI.Data.Interface;
using OLabWebAPI.Model;
using OLabWebAPI.Services;
using OLabWebAPI.Services.TurkTalk;
using OLabWebAPI.TurkTalk.BusinessObjects;
using OLabWebAPI.Utils;
using IOLabSession = OLabWebAPI.Data.Interface.IOLabSession;
using IUserService = OLabWebAPI.Services.IUserService;
using System.Net;
using System;

namespace TurkTalkSvc
{
  public class Startup
  {
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddSignalR(hubOptions =>
            {
              hubOptions.EnableDetailedErrors = true;
              hubOptions.ClientTimeoutInterval = TimeSpan.FromSeconds(90);
              hubOptions.EnableDetailedErrors = true;
            });

      services.AddCors(options =>
      {
        options.AddPolicy("CorsPolicy",
           builder => builder
            // .AllowAnyOrigin()
            .WithOrigins("http://localhost:4000", "http://localhost:3000", "https://dev.olab.ca", "https://demo.olab.ca")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
          );
      });

      services.AddControllers().AddNewtonsoftJson();
      services.AddLogging(builder =>
      {
        IConfigurationSection config = Configuration.GetSection("Logging");
        builder.ClearProviders();
        builder.AddConsole(configure =>
        {
          //configure.FormatterName = ConsoleFormatterNames.Simple;
          configure.FormatterName = ConsoleFormatterNames.Systemd;
        });
        builder.AddConfiguration(config);
      });

      // added for logging
      services.AddHttpLogging(options =>
      {
        options.LoggingFields = HttpLoggingFields.Request;
      });

      // configure strongly typed settings object
      services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

      var ProxyServer = Configuration["AppSettings:ProxyServer"];
      if (string.IsNullOrEmpty(ProxyServer))
        services.Configure<ForwardedHeadersOptions>(options =>
        {
          options.ForwardedHeaders =
              ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });
      else
        services.Configure<ForwardedHeadersOptions>(options =>
        {
          options.ForwardedHeaders =
              ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
          options.KnownProxies.Add(IPAddress.Parse(ProxyServer));
        });

      // Additional code to register the ILogger as a ILogger<T> where T is the Startup class
      services.AddSingleton(typeof(ILogger), typeof(Logger<Startup>));

      var serverVersion = ServerVersion.AutoDetect(Configuration.GetConnectionString(Constants.DefaultConnectionStringName));
      services.AddDbContext<OLabDBContext>(
          dbContextOptions => dbContextOptions
              .UseMySql(Configuration.GetConnectionString(Constants.DefaultConnectionStringName), serverVersion)
              // The following three options help with debugging, but should
              // be changed or removed for production.
              // .LogTo(Console.WriteLine, LogLevel.Information)
              // .EnableSensitiveDataLogging()
              .EnableDetailedErrors()
        );

      // Everything from this point on is optional but helps with debugging.
      // .UseLoggerFactory(
      //     LoggerFactory.Create(
      //         logging => logging
      //             .AddConsole()
      //             .AddFilter(level => level >= LogLevel.Information)))
      // .EnableSensitiveDataLogging()
      // .EnableDetailedErrors()
      // );

      // MoodleJWTService.Setup(Configuration, services);
      OLabJWTService.Setup(Configuration, services);

      services.AddTransient<IUserContext, UserContext>();

      // define instances of application services
      services.AddScoped<IUserService, OLabUserService>();
      services.AddScoped<IOLabSession, OLabSession>();
      services.AddSingleton<Conference>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      // app.UseHttpsRedirection();
      // global cors policy
      app.UseCors("CorsPolicy");
      app.UseRouting();
      app.UseAuthorization();

      // added for logging
      app.UseForwardedHeaders();
      app.UseHttpLogging();

      // custom jwt auth middleware
      app.UseMiddleware<OLabJWTService>();

      // get signalR endpoint
      var signalREndpoint = Configuration["AppSettings:SignalREndpoint"];
      if (string.IsNullOrEmpty(signalREndpoint))
        signalREndpoint = "/turktalk";

      app.UseEndpoints(x =>
      {
        x.MapControllers();
        x.MapHub<TurkTalkHub>(signalREndpoint);
      });

    }
  }
}
