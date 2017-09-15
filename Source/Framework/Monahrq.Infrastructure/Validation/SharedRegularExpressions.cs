using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Monahrq.Infrastructure.Validation
{
    public static class SharedRegularExpressions
    {
        public const string ICD9_CODE_REGEX = "^[evEV0-9]([0-9]{2,4})$";

        public const string ICD10_DIAGNOSTICCODE_REGEX = "^[a-zA-Z][0-9]([a-zA-Z0-9]{1,5})$";

        public const string ICD10_PROCEDURECODE_REGEX = "^([a-zA-Z0-9]{7})$";

        public const string NO_SPECIAL_CHARACTERS_REGEX = "^[a-zA-Z0-9]*$";

        public static string ICD9_PROCEDURECODE_REGEX = "^([0-9]{3,4})$";

        public static string EV_SPECIALCODE_REGEX = "^[evEV].*";

        public static Regex ICD9Regex
        {
            get;
            private set;
        }

        public static Regex ICD9ProcedureRegex
        {
            get;
            private set;
        }

        public static Regex ICD10Regex
        {
            get;
            private set;
        }

        public static Regex ICD10ProcedureRegex
        {
            get;
            private set;
        }

        public static Regex NoSpecialCharactersRegex
        {
            get;
            private set;
        }

        public static Regex EVSpecialCodeRegex
        {
            get;
            private set;
        }

        static SharedRegularExpressions()
        {
            // todo: profile to determine whether compilation is appropriate
            SharedRegularExpressions.ICD9_PROCEDURECODE_REGEX = "^([0-9]{3,4})$";
            SharedRegularExpressions.ICD10ProcedureRegex = new Regex("^([a-zA-Z0-9]{7})$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD10Regex = new Regex("^[a-zA-Z][0-9]([a-zA-Z0-9]{1,5})$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD9Regex = new Regex("^[evEV0-9]([0-9]{2,4})$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD9ProcedureRegex = new Regex(SharedRegularExpressions.ICD9_PROCEDURECODE_REGEX, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.NoSpecialCharactersRegex = new Regex("^[a-zA-Z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.EVSpecialCodeRegex = new Regex("^[evEV].*", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }
}
