using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ImportSessions.Dtos
{
    public class ImportSessionDto : EntityDto<Guid>
    {
        public int NoId { get; set; }
        public string ImportSource { get; set; }
        public string DataSourceType { get; set; }
        public string DataSourceName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedStatus { get; set; }
        public string Notes { get; set; }
    }
}
