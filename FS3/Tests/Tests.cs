using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Bogus;
using Bogus.DataSets;
using Superset.Common;
using Superset.Web.State;

namespace FS3.Tests
{
    public class Tests
    {
        const   int                runOptions = 25000;
        private FlareSelector<int> _fs1;
        private int                cDisabled    = 0;
        private int                cNormal      = 0;
        private int                cPlaceholder = 0;
        private int                cSelected    = 0;

        public void Run()
        {
            IEnumerable<IOption<int>> data = Generate.Contacts(15000).Select(v => v.NameOption);
            Console.WriteLine("Data generated");

            _fs1 = new FlareSelector<int>(() => data, true);

            // while (true)
            // {
            //     Bench1();
            //     Bench2();
            //     // Bench3();
            //
            //     Console.WriteLine();
            // }
        }

        private void Bench1()
        {
            Stopwatch s1 = new Stopwatch();
            s1.Start();
            for (var run = 0; run < runOptions; run++)
            {
                for (var b = 0; b < _fs1.Batches.Length; b++)
                {
                    UpdateTrigger tw = _fs1.Batches[b].Item1;

                    for (var o = 0; o < _fs1.Batches[b].Item2.Count; o++)
                    {
                        IOption<int> d = _fs1.Batches[b].Item2[o];

                        Inner(b, o, d);
                    }
                }
            }

            s1.Stop();
            Console.WriteLine("Bench1 = " + s1.Elapsed);
        }

        private void Bench2()
        {
            Stopwatch s1 = new Stopwatch();
            s1.Start();

            for (var run = 0; run < runOptions; run++)
            {
                var           i      = 0;
                int           cIndex = -1;
                UpdateTrigger cTw    = null;

                foreach (IOption<int> d in _fs1.Options())
                {
                    int batchID = i / _fs1.BatchSize;
                    int o       = i % _fs1.BatchSize;

                    if (cIndex != batchID)
                    {
                        cTw    = _fs1.Batches[batchID].Item1;
                        cIndex = batchID;
                    }

                    Inner(batchID, o, d);

                    i++;
                }
            }

            s1.Stop();
            Console.WriteLine("Bench2 = " + s1.Elapsed);
        }

        // private void Bench3()
        // {
        //     Stopwatch s1 = new Stopwatch();
        //     s1.Start();
        //
        //     for (var run = 0; run < runOptions; run++)
        //     {
        //         foreach ((int batchID, int optionIndex, IOption<int> option) in _fs1.OptionsIndexed())
        //         {
        //             Inner(batchID, optionIndex, option);
        //         }
        //     }
        //
        //     s1.Stop();
        //     Console.WriteLine("Bench3 = " + s1.Elapsed);
        // }

        private void Inner(int batchID, int o, IOption<int> d)
        {
            if (d.Placeholder)
            {
                bool   shown = _fs1.IsOptionShown(d);
                string text  = d.OptionText;
                cPlaceholder++;
            }
            else if (d.Disabled)
            {
                bool   shown = _fs1.IsOptionShown(d);
                string text  = d.OptionText;
                cDisabled++;
            }
            else if (_fs1.IsOptionSelected(d))
            {
                bool   shown = _fs1.IsOptionShown(d);
                string text  = d.OptionText;
                cSelected++;
                string id = $"FS_O_{batchID}_{o}";
            }
            else
            {
                bool   shown = _fs1.IsOptionShown(d);
                string text  = d.OptionText;
                cNormal++;
                string id = $"FS_O_{batchID}_{o}";
            }
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
                OptionText   = $"{FirstName} {LastName} - {PhoneNumber}",
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