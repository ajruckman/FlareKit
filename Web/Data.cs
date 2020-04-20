using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Bogus.DataSets;
using FlareSelect;

// using FlareSelect;

namespace Web
{
    public class Contact
    {
        public int ID { get; set; }

        public DateTime Date        { get; set; }
        public string   FirstName   { get; set; }
        public string   LastName    { get; set; }
        public string   Email       { get; set; }
        public string   PhoneNumber { get; set; }
        public string   Gender      { get; set; }
        
        public Contact() { }

        public Option<int> NameOption =>
            new Option<int>
            {
                ID           = ID,
                OptionText   = $"{FirstName} {LastName} {Email} {PhoneNumber} {Gender} {FirstName} {LastName} {Email} {PhoneNumber} {Gender}",
                SelectedText = $"{FirstName} {LastName}",
                // Selected     = true
            };

        public Option<int> LongOption =>
            new Option<int>
            {
                ID           = ID,
                OptionText   = PhoneNumber,
                SelectedText = PhoneNumber,
                Selected     = true
            };

        public static List<Option<int>> PreGeneratedOptions;
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