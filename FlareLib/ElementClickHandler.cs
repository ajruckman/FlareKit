using System;

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

//            Console.WriteLine("Body called");
            OnOuterClick?.Invoke();
        }

        public void ReactiveClick(string source)
        {
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

//            Console.WriteLine("Element called");
            OnReactiveClick?.Invoke(source);
        }

        public void NonreactiveClick(string source)
        {
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

//            Console.WriteLine("Noninteractive called");
            OnNonreactiveClick?.Invoke(source);
        }
    }
}