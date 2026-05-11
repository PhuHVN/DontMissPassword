using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DontMissPassword.Domain.Common.Results
{
    public sealed class Error
    {
        public string Code { get; }
        public string Message { get; }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }


        public static readonly Error None =
            new("NONE", string.Empty);

        public static readonly Error NotFound =
            new("NOT_FOUND", "Resource not found");

        public static readonly Error Invalid =
            new("INVALID", "Invalid request");

        public static readonly Error Conflict =
            new("CONFLICT", "Resource already exists");

        public static readonly Error Unauthorized =
            new("UNAUTHORIZED", "Unauthorized access");
    }
}
