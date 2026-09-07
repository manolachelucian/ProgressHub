using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressHub.Core.Constants
{
    public static class PhonePrefix
    {
        public static readonly Dictionary<string, string> PrefixOptions = new()
        {
            { "+420", " CZ (+420)" },
            { "+421", " SK (+421)" },
            { "+49",  " DE (+49)" },
            { "+43",  " AT (+43)" },
            { "+48",  " PL (+48)" },
            { "+44",  " UK (+44)" },
            { "+33",  " FR (+33)" },
            { "+39",  " IT (+39)" },
            { "+34",  " ES (+34)" },
            { "+41",  " CH (+41)" },
            { "+31",  " NL (+31)" },
            { "+1",   " US/CA (+1)" }
        };

        public const string DefaultPrefix = "+420";

    }
}
