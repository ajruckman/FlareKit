using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.JSInterop;

namespace FlareLib
{
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class ElementClickHandler
    {
//        private readonly object _mutex = new object();
//        private          int    _blocks;

        public event Action OnOuterClick;
        public event Action<string> OnReactiveClick;
        public event Action<string> OnOuterClickHandled;

        [JSInvokable]
        public void OuterClick()
        {
            Console.WriteLine("OuterClick");
            OnOuterClick?.Invoke();
        }

        [JSInvokable]
        public void ReactiveClick(string uid)
        {
            Console.WriteLine("ReactiveClick -> " + uid);
            OnReactiveClick?.Invoke(uid);
        }

        public void OuterClickHandled(string uid)
        {
//            Thread.Sleep(10);
            Console.WriteLine("OuterClickHandled");
            OnOuterClickHandled?.Invoke(uid);
        }

//        public event Action         OnOuterClick;
//        public event Action<string> OnReactiveClick;
//        public event Action<string> OnNonreactiveClick;
//
//        public void BlockOne()
//        {
//            lock (_mutex)
//            {
//                _blocks++;
//            }
//        }
//
//        [JSInvokable]
//        public void OuterClick()
//        {
//            Console.WriteLine("OuterClick");
//            lock (_mutex)
//            {
//                if (_blocks > 0)
//                {
//                    _blocks--;
//                    return;
//                }
//            }
//
//            OnOuterClick?.Invoke();
//        }
//
//
//        public void ReactiveClick(string source)
//        {
//            lock (_mutex)
//            {
//                if (_blocks > 0)
//                {
//                    _blocks--;
//                    return;
//                }
//            }
//
//            OnReactiveClick?.Invoke(source);
//        }
//
//        public void NonreactiveClick(string source)
//        {
//            lock (_mutex)
//            {
//                if (_blocks > 0)
//                {
//                    _blocks--;
//                    return;
//                }
//            }
//
//            OnNonreactiveClick?.Invoke(source);
//        }
    }
}