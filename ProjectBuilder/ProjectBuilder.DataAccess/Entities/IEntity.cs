using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public interface IEntity<T>
    {
        public T EntityId { get; set; }
    }
}
