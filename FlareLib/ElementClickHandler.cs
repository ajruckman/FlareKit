using System;

namespace FlareLib
{
    public sealed class ElementClickHandler
    {
        private readonly object _mutex = new object();
        private          int    _blocks;

        public event Action         OnBodyClick;
        public event Action<string> OnElementClick;
        public event Action<string> OnNoninteractiveClick;

        public void BlockOne()
        {
            lock (_mutex)
            {
                Console.WriteLine($"     {_blocks} -> {++_blocks}");
            }
        }

        public void BodyClicked()
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    Console.WriteLine("Body locked");
                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            Console.WriteLine("Body called");
            OnBodyClick?.Invoke();
        }

        public void ElementClicked(string source)
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    Console.WriteLine("Element locked");
                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            Console.WriteLine("Element called");
            OnElementClick?.Invoke(source);
        }

        public void NoninteractiveClicked(string source)
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    Console.WriteLine("Noninteractive locked");
                    Console.WriteLine($"{_blocks} -> {--_blocks}");
                    return;
                }
            }

            Console.WriteLine("Noninteractive called");
            OnNoninteractiveClick?.Invoke(source);
        }
    }
}