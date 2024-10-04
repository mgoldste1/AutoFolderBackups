using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hammerwatch_backupCore
{
    public static class Extensions
    {
        public static bool ToBool(this string? s)
        {
            if (s == null)
                throw new ArgumentNullException();
            else if (bool.TryParse(s, out bool b))
                return b;
            else if (s.Trim() == "0")
                return false;
            else if (s.Trim() == "1")
                return true;
            else
                throw new ArgumentException("Can't convert to bool");
        }
    }
}
