using System;

namespace FlareLib
{
    public class ElementClickHandler
    {
        public event Action OnBodyClick;
        public event Action<string> OnElementClick;

        private bool _blocked;

        public void BlockOne()
        {
            _blocked = true;
        }
        
        public void BodyClicked()
        {
            if (_blocked)
            {
                _blocked = false;
                return;
            }

            OnBodyClick?.Invoke();
        }

        public void ElementClicked(string source)
        {
            OnElementClick?.Invoke(source);
        }
    }
}