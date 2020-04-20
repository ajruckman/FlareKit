using System;
using System.Collections.Generic;
using System.Linq;
using FlareSelect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Superset.Logging;
using Web.Pages;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("Generating fake data... ");
            RecordCache.Records = RecordCache.FakeData(1_000);

            Contact.PreGeneratedOptions = new List<Option<int>>();
            
            // Contact.PreGeneratedOptions.Add(new Option<int>
            // {
            //     ID = -1,
            //     OptionText =
            //         "Contact.PreGeneratedOptions = Generate.Contacts(1_000).Select(v => v.NameOption).ToList();",
            //     SelectedText =
            //         "Contact.PreGeneratedOptions = Generate.Contacts(1_000).Select(v => v.NameOption).ToList();",
            //     Selected = false,
            // });

            Contact.PreGeneratedOptions.Add(new Option<int>
            {
                ID          = -2,
                OptionText  = "This is a palceholder",
                Placeholder = true
            });
            Contact.PreGeneratedOptions.Add(new Option<int>
            {
                ID         = -3,
                OptionText = "This is disabled",
                Disabled   = true
            });

            Contact.PreGeneratedOptions.AddRange(Generate.Contacts(1_000).Select(v => v.NameOption).ToList());
            // for (var i = 3; i < 30; i += 2)
            // {
            //     Contact.PreGeneratedOptions[i].Selected = true;
            // }


            Console.WriteLine("complete");

            // Log.LogUpdates = true;

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