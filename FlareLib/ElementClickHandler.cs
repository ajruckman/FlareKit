using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.JSInterop;

namespace FlareLib
{
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class ElementClickHandler
    {
        public event Action<string> OnInnerClick;
        public event Action<string> OnOuterClick;

        [JSInvokable]
        public void InnerClick(string targetID)
        {
            OnInnerClick?.Invoke(targetID);
        }

        [JSInvokable]
        public void OuterClick(string targetID)
        {
            OnOuterClick?.Invoke(targetID);
        }
    }
}