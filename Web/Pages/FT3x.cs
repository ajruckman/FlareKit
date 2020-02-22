using System.Collections.Generic;
using Bogus;
using FT3;

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

        private FlareTable<Record> _flareTable1;

        protected override void OnInitialized()
        {
            List<Record> data = FakeData(100);
            _flareTable1 = new FlareTable<Record>(() => data);

            _flareTable1.RegisterColumn(nameof(Record.Name), shown: true, sortDirection: SortDirections.Ascending);
            _flareTable1.RegisterColumn(nameof(Record.Age),  filterValue: "1");
            _flareTable1.RegisterColumn(nameof(Record.City));
            _flareTable1.RegisterColumn(nameof(Record.State));
            _flareTable1.RegisterColumn(nameof(Record.Country), shown: false);
            _flareTable1.RegisterColumn(nameof(Record.Zip));
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
    }
}