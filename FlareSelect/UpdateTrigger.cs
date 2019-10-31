using System;

// ReSharper disable MemberCanBeInternal

namespace FlareSelect
{
    public sealed class UpdateTrigger
    {
        public event Action OnUpdate;

        public void Trigger()
        {
            OnUpdate?.Invoke();
        }
    }
}