using System;
using System.Diagnostics;

namespace FlareLib
{
    public sealed class ElementClickHandler
    {
        private readonly object _mutex = new object();
        private          int    _blocks;

        public event Action         OnOuterClick;
        public event Action<string> OnReactiveClick;
        public event Action<string> OnNonreactiveClick;

        public void BlockOne()
        {
            lock (_mutex)
            {
                _blocks++;
//                Console.WriteLine($"     {_blocks} -> {++_blocks}");
            }
        }

        public void OuterClick()
        {
            Debug.WriteLine("OuterClick called");

            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
//                    Console.WriteLine("Body locked");
//                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            OnOuterClick?.Invoke();
        }

        public void ReactiveClick(string source)
        {
            Debug.WriteLine("ReactiveClick called");

            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
//                    Console.WriteLine("Element locked");
//                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            OnReactiveClick?.Invoke(source);
        }

        public void NonreactiveClick(string source)
        {
            Debug.WriteLine("NonreactiveClick called");

            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
//                    Console.WriteLine("Noninteractive locked");
//                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            OnNonreactiveClick?.Invoke(source);
        }
    }
}