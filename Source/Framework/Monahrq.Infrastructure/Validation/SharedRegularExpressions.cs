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
        public const string ICD9_CODE_REGEX = "\\b([evEV0-9]{1}[0-9]{1,5}[^/\\\\()~!@#$%^&*_])$";

        public const string ICD10_DIAGNOSTICCODE_REGEX = "\\b([a-zA-Z]{1}[0-9]{2})([a-zA-Z0-9]{0,7}[^/\\\\()~!@#$%^&*_])$";

        public const string ICD10_PROCEDURECODE_REGEX = "\\b([a-zA-Z0-9]{6,7}[^/\\\\()~!@#$%^&*_])$";

        public const string NO_SPECIAL_CHARACTERS_REGEX = "^[a-zA-Z0-9]*$";

        public static string ICD9_PROCEDURECODE_REGEX;

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

        static SharedRegularExpressions()
        {
            // todo: profile to determine whether compilation is appropriate
            SharedRegularExpressions.ICD9_PROCEDURECODE_REGEX = "\\b(^[0-9]{2,5}[^/\\\\()~!@#$%^&*_])+$";
            SharedRegularExpressions.ICD10ProcedureRegex = new Regex("\\b([a-zA-Z0-9]{6,7}[^/\\\\()~!@#$%^&*_])$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD10Regex = new Regex("\\b([a-zA-Z]{1}[0-9]{2})([a-zA-Z0-9]{0,7}[^/\\\\()~!@#$%^&*_])$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD9Regex = new Regex("\\b([evEV0-9]{1}[0-9]{1,5}[^/\\\\()~!@#$%^&*_])$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.ICD9ProcedureRegex = new Regex(SharedRegularExpressions.ICD9_PROCEDURECODE_REGEX, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
            SharedRegularExpressions.NoSpecialCharactersRegex = new Regex("^[a-zA-Z0-9]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }
    }
}
