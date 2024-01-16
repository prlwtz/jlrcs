using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jlr_sample
{
    public static class StringEx
    {
        public static string RightString(this string str, int length)
        {
            return str.Substring(str.Length - length);
        }
    }
}
