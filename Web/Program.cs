using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Web.Pages;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Generating fake data... ");
            RecordCache.Records = RecordCache.FakeData(1_000);
            Console.WriteLine("complete");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                
                .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                     webBuilder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                 });
    }
}