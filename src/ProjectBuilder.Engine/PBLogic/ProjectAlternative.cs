using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBLogic
{
   
    public class ProjectAlternative
    {
        public int AltNo;
        public int ShiftFromPreferred;
        public int FirstYearWork;
        public double WorkingUtility;
        public bool IsPreferred;
        public SortedDictionary<string, double>[] Costs = { new SortedDictionary<string, double>(), new SortedDictionary<string, double>() };
        // One for asset types ('P' or 'B'), one for 'R'
    }
}
