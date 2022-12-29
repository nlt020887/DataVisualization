using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
              .ConfigureWebHostDefaults(webBuilder =>
              {
                  webBuilder.UseStartup<Startup>();
                  webBuilder.UseSerilog((_, config) =>
                  {
                      config
                          .MinimumLevel.Information()
                          .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                          .Enrich.FromLogContext()
                          .WriteTo.File(@"Logs/log.txt", rollingInterval: RollingInterval.Day);
                  });
                 
              })
              .ConfigureAppConfiguration((hostingContext, config) =>
              {
                  config
                  .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                  .AddJsonFile("OcelotConfiguration.json", optional: false, reloadOnChange: true);
              });
            
                    
            
    }
}
