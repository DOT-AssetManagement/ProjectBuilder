using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    public enum DialogResult
    {
        Ok,
        Cancel,
        Retry,
        Skip
    }
    public enum ComparesionType
    {
        GreaterThen,
        LessThen,
        GreateOrEqual,
        LessOrEqaul,
        GreaterAndLess,
        GreaterOrEqualLessOrEqual,
    }

    public enum Roles
    {
        SuperAdmin,
        Admin,
        Moderator,
        Basic
    }
}
