using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Bogus;
using Bogus.DataSets;

namespace FlareSelect
{
    public class Contact
    {
        public int ID { get; set; }

        public string FirstName   { get; set; }
        public string LastName    { get; set; }
        public string Email       { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender      { get; set; }

        public Option NameOption =>
            new Option
            {
                ID            = ID,
                DropdownValue = $"{FirstName} {LastName}",
                SelectedValue = FirstName,
                Selected      = false
            };

        public Option LongOption =>
            new Option
            {
                ID            = ID,
                DropdownValue = PhoneNumber,
                SelectedValue = PhoneNumber,
                Selected      = true
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
                 .RuleFor(c => c.Gender,      f => f.PickRandom<Name.Gender>().ToString())
                 .RuleFor(c => c.FirstName,   f => f.Name.FirstName())
                 .RuleFor(c => c.LastName,    f => f.Name.LastName())
                 .RuleFor(c => c.PhoneNumber, f => string.Concat(Enumerable.Repeat(f.Phone.PhoneNumber(), 10)))
                 .RuleFor(c => c.Email,       f => f.Internet.ExampleEmail());

            return faker.Generate(amount);
        }
    }
}