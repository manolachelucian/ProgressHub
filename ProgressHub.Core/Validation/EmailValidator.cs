using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProgressHub.Core.Validation
{
    public static partial class EmailValidator
    {

        // 1. Před @: alfanumerické bloky oddělené max jednou tečkou (žádné .. ani tečka na začátku/konci)
        // 2. Za @: doménové štítky ohraničené tečkou (žádné .. a platná TLD o délce min 2 znaky)
        [GeneratedRegex(@"^[a-zA-Z0-9]+(?:[._%+-][a-zA-Z0-9]+)*@[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*(?:\.[a-zA-Z0-9]+(?:-[a-zA-Z0-9]+)*)*\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex EmailRegex();


        public static bool IsValid(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) {return false;}
            return EmailRegex().IsMatch(email);
        }

    }
}
