using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.UserClaimEntity.Dtos
{
    public class UpdateUserClaimDto : EntityDto<int>
    {
        public long UserId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
