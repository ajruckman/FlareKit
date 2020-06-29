using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FlareTables;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Pages
{
    public static class RecordCache
    {
        public static List<Record> Records { get; set; }

        public static List<Record> FakeData(int n)
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

    public class Record
    {
        public string Name    { get; set; }
        public int    Age     { get; set; }
        public string City    { get; set; }
        public string State   { get; set; }
        public string Country { get; set; }
        public string Zip     { get; set; }

        public Record() { }
    }

    public partial class FT3x
    {
        private FlareTable<Record> _dummyFlareTable;
        private FlareTable<Record> _flareTable1;

        protected override void OnInitialized()
        {
            _dummyFlareTable = new FlareTable<Record>(() => RecordCache.Records);

            _flareTable1 = new FlareTable<Record>(
                () => RecordCache.Records,
                new SessionStorageProvider(SessionStorage),
                // new LocalStorageProvider(LocalStorage),
                "ft3x",
                fixedLayout: true,
                rowColorGetter: row =>
                    row.Zip.StartsWith("0") || row.Name.StartsWith("A") ? RowColor.Undefined :
                    row.Zip.Contains("-")                               ? RowColor.Red : RowColor.Green,
                clickable: true
            );

            _flareTable1.RegisterColumn(nameof(Record.Name),    shown: true, sortDirection: SortDirections.Ascending);
            _flareTable1.RegisterColumn(nameof(Record.Age),     filterValue: "1", width: "60px");
            _flareTable1.RegisterColumn(nameof(Record.City),    filterable: false, sortable: false);
            _flareTable1.RegisterColumn(nameof(Record.State),   sortable: false);
            _flareTable1.RegisterColumn(nameof(Record.Country), shown: false, filterable: false, displayName: "");
            _flareTable1.RegisterColumn(nameof(Record.Zip),     shown: false, monospace: true,   width: "120px");

            _flareTable1.OnRowClick += record =>
                Console.WriteLine($"Click: {record.Name}");

            _flareTable1.OnRowClickDetail += (record, ctrl, shift, mid) =>
                Console.WriteLine($"Click detail: {record.Name} | Ctrl: {ctrl} | Shift: {shift} | Mid: {mid}");
        }

        protected override async Task OnInitializedAsync()
        {
            await _flareTable1.LoadStorageValues();
        }

        private void Download(MouseEventArgs args)
        {
            string csv = _flareTable1.AsCSV();
            Superset.Web.Utilities.Utilities.SaveAsFile(JSRuntime, $"dump_{DateTime.Now.Ticks}.csv",
                Encoding.ASCII.GetBytes(csv));
        }
    }
}