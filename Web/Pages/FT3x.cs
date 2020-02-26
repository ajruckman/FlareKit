using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FT3;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Pages
{
    public partial class FT3x
    {
        private class Record
        {
            public string Name    { get; set; }
            public int    Age     { get; set; }
            public string City    { get; set; }
            public string State   { get; set; }
            public string Country { get; set; }
            public string Zip     { get; set; }
        }

        private FlareTable<Record> _dummyFlareTable;
        private FlareTable<Record> _flareTable1;

        protected override async Task OnInitializedAsync()
        {
            List<Record> data = FakeData(100);

            _dummyFlareTable = new FlareTable<Record>(() => data);
            _flareTable1     = new FlareTable<Record>(() => data, SessionStorage, "ft3x");

            await _flareTable1.RegisterColumn(nameof(Record.Name), shown: true, sortDirection: SortDirections.Ascending);
            await _flareTable1.RegisterColumn(nameof(Record.Age), filterValue: "1");
            await _flareTable1.RegisterColumn(nameof(Record.City));
            await _flareTable1.RegisterColumn(nameof(Record.State));
            await _flareTable1.RegisterColumn(nameof(Record.Country), shown: false);
            await _flareTable1.RegisterColumn(nameof(Record.Zip));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await _flareTable1.LoadSessionValues();
        }

        private static List<Record> FakeData(int n)
        {
            Faker<Record> faker = new Faker<Record>();
            faker.UseSeed(0);

            faker.RuleFor(v => v.Name, f => f.Name.FullName())
                 .RuleFor(v => v.Age,     f => f.Person.DateOfBirth.Hour)
                 .RuleFor(v => v.City,    f => f.Address.City())
                 .RuleFor(v => v.State,   f => f.Address.State())
                 .RuleFor(v => v.Country, f => f.Address.Country())
                 .RuleFor(v => v.Zip,     f => f.Address.ZipCode());

            return faker.Generate(n);
        }

        private void Download(MouseEventArgs args)
        {
            string csv = _flareTable1.AsCSV();
            Console.WriteLine(csv);
            Superset.Web.Utilities.Utilities.SaveAsFile(JSRuntime, $"dump_{DateTime.Now.Ticks}.csv",
                Encoding.ASCII.GetBytes(csv));
        }
    }
}