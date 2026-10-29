using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBLogic
{
    public class BudgetAttributeCriterion
    {
        public string? AttributeName;
        public string Operation = "=";
        public string? AttributeValue;

        public override string ToString()
        {
            return $"[{AttributeName}][{Operation}][{AttributeValue}]";
        }
    }
}
