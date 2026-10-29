using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Models.DefaultSlack.Dtos
{
    public class CreateDefaultSlackDto
    {
        public string Id { get; set; }
        public int MoveBefore { get; set; }
        public int MoveAfter { get; set; }
    }
}
