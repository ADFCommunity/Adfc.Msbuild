using System.Collections.Generic;
using System.Linq;

namespace Adfc.Msbuild
{
    public class ErrorCodes
    {
        /// <summary>Configuration JPath wasn't found in document.</summary>
        public static ErrorCodes Adfc0001 = new ErrorCodes("ADFC0001", ErrorSeverity.Warning);

        /// <summary>File specified in configuration wasn't found.</summary>
        public static ErrorCodes Adfc0003 = new ErrorCodes("ADFC0002", ErrorSeverity.Warning);

        public static IReadOnlyDictionary<string, ErrorCodes> All =
            new[] { Adfc0001, Adfc0003 }
            .ToDictionary(e => e.Code, e => e);

        public string Code { get; }
        public ErrorSeverity DefaultSeverity { get; }

        public ErrorCodes(string code, ErrorSeverity severity)
        {
            Code = code;
            DefaultSeverity = severity;
        }
    }
}
