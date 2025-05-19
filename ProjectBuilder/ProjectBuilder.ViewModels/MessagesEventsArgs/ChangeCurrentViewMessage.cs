using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class ChangeCurrentViewMessage
    {
        public ChangeCurrentViewMessage(CurrentView currentView)
        {
            CurrentView = currentView;
        }
        public CurrentView CurrentView { get;}
    }
}
