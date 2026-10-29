using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace ProjectBuilder.Models.ImportSessions.Dtos
{
    public class UpdateImportSessionDto : EntityDto<Guid>
    {
        public string ImportSource { get; set; }
        public string DataSourceType { get; set; }
        public string DataSourceName { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CompletedStatus { get; set; }
        public string Notes { get; set; }
    }
}
