using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Bogus.DataSets;
using Microsoft.AspNetCore.Components.Rendering;
using Superset.Common;
using Superset.Web.State;

namespace FS3.Tests
{
    public class Tests
    {
        public void Run()
        {
            IEnumerable<IOption<int>> data = Generate.Contacts(931).Select(v => v.NameOption);

            FlareSelector<int> _fs1 = new FlareSelector<int>(() => data, true);

            foreach ((UpdateTrigger, List<IOption<int>>) batch in _fs1.Batches)
            {
                foreach (var option in batch.Item2)
                {
                    Console.WriteLine($"\t{option.ID}");
                }
            }

            // foreach (Option<int> option in _fs1.GenerateBatches())
            // {
            // Console.WriteLine(option.BatchID);
            // }
        }
    }

    public class Option2<T> : IOption<T> where T : IEquatable<T>
    {
        public T      ID           { get; set; }
        public string OptionText   { get; set; }
        public string SelectedText { get; set; }
        public bool   Selected     { get; set; }
        public bool   Disabled     { get; set; }
        public bool   Placeholder  { get; set; }
    }

    public class Contact
    {
        public int ID { get; set; }

        public DateTime Date        { get; set; }
        public string   FirstName   { get; set; }
        public string   LastName    { get; set; }
        public string   Email       { get; set; }
        public string   PhoneNumber { get; set; }
        public string   Gender      { get; set; }

        public Option2<int> NameOption =>
            new Option2<int>
            {
                ID           = ID,
                OptionText   = $"{FirstName} {LastName}",
                SelectedText = FirstName,
                Selected     = true
            };

        public Option2<int> LongOption =>
            new Option2<int>
            {
                ID           = ID,
                OptionText   = PhoneNumber,
                SelectedText = PhoneNumber,
                Selected     = true
            };
    }

    public static class Generate
    {
        public static List<Contact> Contacts(int amount)
        {
            Faker<Contact> faker =
                new Faker<Contact>();

            faker.UseSeed(0);

            faker.RuleFor(c => c.ID, f => f.IndexGlobal)
                 .RuleFor(c => c.Date,        f => f.Date.Past(300))
                 .RuleFor(c => c.Gender,      f => f.PickRandom<Name.Gender>().ToString())
                 .RuleFor(c => c.FirstName,   f => f.Name.FirstName())
                 .RuleFor(c => c.LastName,    f => f.Name.LastName())
                 .RuleFor(c => c.PhoneNumber, f => string.Concat(Enumerable.Repeat(f.Phone.PhoneNumber(), 3)))
                 .RuleFor(c => c.Email,       f => f.Internet.ExampleEmail());

            return faker.Generate(amount);
        }
    }
}