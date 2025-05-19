using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Core.Models
{
    public class ImportSessionsModel
    {
        public int NoId { get; set; }
        public Guid Id { get; set; }
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
