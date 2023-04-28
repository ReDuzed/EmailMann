using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteEmail
{
    public static class MoreInfo
    {
        //  class was being used for a starting many processes,
        //  but to avoid this, SMTP handling was added to this console app
        public static ProcessStartInfo GetInfo(string exe, string[] args)
        {
            var info = new ProcessStartInfo(exe, "");
            for (int i = 0; i < args.Length; i++)
            {
                info.Arguments += args[i] + " ";
                info.Arguments.TrimEnd(' ');
            }
            return info;
        }
    }
}
