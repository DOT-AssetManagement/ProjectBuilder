using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.DefaultSlack.Dtos
{
    public class DefaultSlackDto : EntityDto<string>
    {
        public int MoveBefore { get; set; }
        public int MoveAfter { get; set; }
    }
}
