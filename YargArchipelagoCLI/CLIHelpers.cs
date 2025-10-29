using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YargArchipelagoCommon;

namespace YargArchipelagoCLI
{
    internal class CLIHelpers
    {
        public static T NextEnumValue<T>(T value) where T : struct, Enum
        {
            var values = (T[])Enum.GetValues(typeof(T));
            int index = Array.IndexOf(values, value);
            return values[(index + 1) % values.Length];
        }
    }
}
