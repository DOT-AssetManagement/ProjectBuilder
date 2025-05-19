

using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class Constants
    {
        public const string MAINDIALOGIDENTIFIER = "RootDialog";
        public const string DATABASECONNECTIONFAILEDMESSAGE = "Could not connect to the database";
        public const string DATABASECONNECTIONSUCCEEDEDMESSAGE = "Connection to the database has been successfully established";
        public const string SUCCESSBRUSH = "SuccessBrush";
        public const string SECONDARYBRUSH = "SecondaryHueMidBrush";
        public const string PRIMARYFOREGROUNDBRUSH = "MaterialDesignBody";
        public const string PAPERBRUSh = "MaterialDesignPaper";
        public const string WARNINGFOREGROUNDBRUSH = "WarningBrush";
        public const string ERRORBRUSH = "ErrorDarkBrush";
        public const string REGEXFLOATNUMBER = @"[+]?((\d+\.?\d*)|(\.\d+))";
        public const string REGEXNUMBER = "^[0-9]+$";
        public const string NUMBEREXPECTED = "Number expected";
        public const string REQUIREDFIELD = "This field is required";
    }
}
