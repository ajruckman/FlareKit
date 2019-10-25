using FlareLib;
using Microsoft.JSInterop;

namespace FlareSelect
{
    public static class Global
    {
        public static ElementClickHandler ElementClickHandler { get; }
            = new ElementClickHandler();
    }
}