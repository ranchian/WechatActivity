using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Unit
{
    public static class StringExt
    {
        public static int ToInt(this string str)
        {
            int i;
            int.TryParse(str, out i);
            return i;

        }

    }
}
