using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.ImportSessions.Dtos
{
    public class CreateImportSessionDto
    {
        public string ImportSource { get; set; }
        public string DataSourceType { get; set; }
        public string DataSourceName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Notes { get; set; }
    }
}
