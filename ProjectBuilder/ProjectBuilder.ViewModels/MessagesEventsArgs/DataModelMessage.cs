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
    public class DataModelMessage<TData>
    {
        public TData Parameter { get;}
        public CurrentView SenderView { get;}
        public DataModelMessage(TData parameter)
        {
            Parameter = parameter;
        }
        public DataModelMessage(TData parameter,CurrentView senderView)
        {
            Parameter = parameter;
            SenderView = senderView;
        }
    }
}
