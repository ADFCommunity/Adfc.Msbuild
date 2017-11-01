using System.Collections.Generic;
using System.Linq;

namespace Adfc.Msbuild
{
    public class ErrorCodes
    {
        /// <summary>Configuration JPath wasn't found in document.</summary>
        public static readonly ErrorCodes Adfc0001 = new ErrorCodes("ADFC0001", ErrorSeverity.Warning);

        /// <summary>File specified in configuration wasn't found.</summary>
        public static readonly ErrorCodes Adfc0002 = new ErrorCodes("ADFC0002", ErrorSeverity.Warning);

        /// <summary>Unable to determine JSON artefact type.</summary>
        public static readonly ErrorCodes Adfc0003 = new ErrorCodes("ADFC0003", ErrorSeverity.Error);

        /// <summary>Unable to parse JSON.</summary>
        public static readonly ErrorCodes Adfc0004 = new ErrorCodes("ADFC0004", ErrorSeverity.Error);

        /// <summary>Failed JSON schema validation.</summary>
        public static readonly ErrorCodes Adfc0005 = new ErrorCodes("ADFC0005", ErrorSeverity.Error);

        /// <summary>Failed implicit JSON schema validation.</summary>
        public static readonly ErrorCodes Adfc0006 = new ErrorCodes("ADFC0006", ErrorSeverity.Error);

        /// <summary>Error during JSON schema validation.</summary>
        public static readonly ErrorCodes Adfc0007 = new ErrorCodes("ADFC0007", ErrorSeverity.Error);

        /// <summary>Artefact name is not unique.</summary>
        public static readonly ErrorCodes Adfc0008 = new ErrorCodes("ADFC0008", ErrorSeverity.Error);

        public static IReadOnlyDictionary<string, ErrorCodes> All =
            new[] { Adfc0001, Adfc0002, Adfc0003, Adfc0004, Adfc0005, Adfc0006, Adfc0007, Adfc0008 }
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
