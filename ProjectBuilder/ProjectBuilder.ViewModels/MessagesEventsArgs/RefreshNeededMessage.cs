using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class RefreshNeededMessage
    {
        public RefreshNeededMessage(bool refreshNeeded)
        {
            RefreshNeeded = refreshNeeded;
        }
        public bool RefreshNeeded { get; }
    }
}
