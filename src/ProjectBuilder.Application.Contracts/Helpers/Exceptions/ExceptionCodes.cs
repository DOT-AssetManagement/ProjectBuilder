using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectBuilder.Helpers.Exceptions
{
    public static class ExceptionCodes
    {
        public const string NotFound = "App:NotFound";
        public const string BusinessRule = "App:BusinessRule";
        public const string Forbidden = "App:Forbidden";
        public const string Internal = "App:Internal";

        public const string Validation = "App:Validation";
        public const string Unauthorized = "App:Unauthorized";
        public const string Conflict = "App:Conflict";
        public const string BadRequest = "App:BadRequest";
        public const string Timeout = "App:Timeout";
        public const string Database = "App:Database";
    }
}
