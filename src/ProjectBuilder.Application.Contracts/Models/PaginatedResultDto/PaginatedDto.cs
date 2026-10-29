using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.PaginatedResultDto
{
    public  class PaginatedDto<T>
    {
        public string SEcho { get; set; }
        public int ITotalRecords { get; set; }
        public int ITotalDisplayRecords { get; set; }
        public List<T> AaData { get; set; }

    }
}
