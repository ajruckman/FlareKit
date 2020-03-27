using System.Linq;
using FS3;

namespace Web.Pages
{
    public partial class FS3x
    {
        private FlareSelector<int> _fs1;
        private FlareSelector<int> _fs2;

        protected override void OnInitialized()
        {
            _fs1 = new FlareSelector<int>(() => Contact.PreGeneratedOptions, false, clearOnSelect: false, minFilterValueLength: 2,
                minFilterValueNotice: "Type 2+ characters", emptyPlaceholder:"Click to select a person");
            _fs2 = new FlareSelector<int>(() => Contact.PreGeneratedOptions, true, clearOnSelect: false,
                emptyPlaceholder:"Click to select a person");
        }
    }
}