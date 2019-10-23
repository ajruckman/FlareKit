using System;
using System.Diagnostics.CodeAnalysis;

namespace FlareLib
{
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class ElementClickHandler
    {
        private readonly object _mutex = new object();
        private          int    _blocks;

        public event Action         OnOuterClick;
        public event Action         OnOuterClickHandled;
        public event Action<string> OnReactiveClick;
        public event Action<string> OnNonreactiveClick;

        public void BlockOne()
        {
            lock (_mutex)
            {
                _blocks++;
            }
        }

        public void OuterClick()
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
                    return;
                }
            }

            OnOuterClick?.Invoke();
        }

        public void OuterClickHandled()
        {
            OnOuterClickHandled?.Invoke();
        }

        public void ReactiveClick(string source)
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
                    return;
                }
            }

            OnReactiveClick?.Invoke(source);
        }

        public void NonreactiveClick(string source)
        {
            lock (_mutex)
            {
                if (_blocks > 0)
                {
                    _blocks--;
                    return;
                }
            }

            OnNonreactiveClick?.Invoke(source);
        }
    }
}