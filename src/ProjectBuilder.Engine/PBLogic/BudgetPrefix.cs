using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618

namespace PBLogic
{
    public class BudgetPrefix
    {
        public string District;
        public string IsInterstate;
        public string AssetType;

        public double? Cost = null;

        public Dictionary<string, Dictionary<string,double>> AttributeCosts = new Dictionary<string, Dictionary<string, double>>();
        public override string ToString()
        {
            return $"D{District}I{IsInterstate}T{AssetType}";
        }
    }
}
