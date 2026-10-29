using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8618

namespace PBLogic
{
    public class CargoValue
    {
        public string CargoType;
        public string? TextValue = null;
        public double? NumericValue = null;
    }

    public class NarrowLibraryTreatment
    {
        public int? District = null;         // These nullable members are not used for filtering
        public int? IsInterstate = null;     // but used for budgeting
        public string? AssetType = null;
        public double? Cost = null;
        public int? PreferredYear = null;
        public bool IsCommitted = false;
        public int PriorityOrder = 4;
        public int DelayFromFirstYearOfProject = 0;
        public Dictionary <string, CargoValue> Cargo = new Dictionary <string, CargoValue> ();
    }
}
