using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LVD.ServiceStackRoutedCacheClient.Example
{
   public class Program
   {
      public static void Main(string[] args)
      {
         CreateWebHostBuilder(args)
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
               configBuilder.SetBasePath(Directory.GetCurrentDirectory());

               configBuilder.AddJsonFile($"appsettings.json",
                   optional: false,
                   reloadOnChange: true);

               configBuilder.AddJsonFile($"appsettings{hostContext.HostingEnvironment.EnvironmentName}.json",
                   optional: true,
                   reloadOnChange: true);
            })
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<Startup>()
            .Build()
            .Run();
      }

      public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
          WebHost.CreateDefaultBuilder(args)
              .UseStartup<Startup>();
   }
}
