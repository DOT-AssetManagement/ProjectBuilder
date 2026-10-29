using System;
using Newtonsoft.Json;

namespace DEALib
{
    /// <summary>
    /// Project object, i.e. entity that needs to be proritized against others and be subjected to selection under constrained inputs.
    /// </summary>
    public class Project : IComparable
    {
        [JsonProperty (PropertyName = "Name", Order = 10, NullValueHandling = NullValueHandling.Ignore)]
        public string Name;         // Project name or ID from the CSV file

        [JsonProperty (PropertyName = "Cost", Order = 20, NullValueHandling = NullValueHandling.Ignore)]
        public double[] Cost;       // Input

        [JsonProperty(PropertyName = "Benefit", Order = 30, NullValueHandling = NullValueHandling.Ignore)]
        public double[] Benefit;    // Output

        [JsonProperty(PropertyName = "U", Order = 40, NullValueHandling = NullValueHandling.Ignore)]
        public double[] U;  // Coefficients for the "output" (benefit)

        [JsonProperty(PropertyName = "V", Order = 50, NullValueHandling = NullValueHandling.Ignore)]
        public double[] V;  // coefficients for "input" (costs)

        [JsonProperty(PropertyName = "RelativeEfficiency", Order = 60, NullValueHandling = NullValueHandling.Ignore)]
        public double? R;    // Relative efficiency

        [JsonProperty(PropertyName = "Selected", Order = 70, NullValueHandling = NullValueHandling.Ignore)]
        public bool? Sel;   // True if project has been selected under constraints, False - if not, null - if cosntrais have never been applied

        [JsonProperty(PropertyName = "Approximate", Order = 80, NullValueHandling = NullValueHandling.Ignore)]
        public char Approx;
        
        public Project()
        {
            Name = null;
            Cost = null;
            Benefit = null;
            U = null;
            V = null;
            R = null;
            Sel = null;
            Approx = '\0';
        }

        public Project(string name, double[] cost, double[] benefit)
        {
            Name = name;
            Approx = '\0';
            if (cost != null && cost.Length > 0)
            {
                Cost = new double[cost.Length];
                cost.CopyTo(Cost, 0);
                V = new double[cost.Length];
            }
            if (benefit != null && benefit.Length > 0)
            {
                Benefit = new double[benefit.Length];
                benefit.CopyTo(Benefit, 0);
                U = new double[benefit.Length];
            }
        }

        /// <summary>
        /// Delegate comparsion method used when the list of projects is sorted by relative effectiveness
        /// When there is a tie and there is only one cost input then the project with the lower cost wins.
        /// </summary>
        /// <param name="obj">Project being comapred with</param>
        /// <returns>-1, 0, +1 as with any other comparator</returns>
        public int CompareTo(object obj)
        {
            Project p = obj as Project;

            if (p.R > this.R)
            {
                return 1;
            }
            else if (p.R < this.R)
            {
                return -1;
            }
            else if (Cost != null && Cost.Length == 1)
            {
                if (p.Cost[0] < Cost[0])
                {
                    return 1;
                }
                else if (p.Cost[0] > Cost[0])
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }

    }
}
