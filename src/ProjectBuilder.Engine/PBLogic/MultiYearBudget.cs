using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using log4net;

namespace PBLogic
{
    public class MultiYearBudget
    {
        public Dictionary<int, Dictionary<string, BudgetPrefix>> Budget = new Dictionary<int, Dictionary<string, BudgetPrefix>>();
        public void Clear() { Budget.Clear(); }

        public MultiYearBudget()
        {
            Clear();
        }

        public void Copy(MultiYearBudget other, bool zeroOut = false)
        {
            Budget.Clear();
            foreach (var entry in other.Budget)
            {
                int yr = entry.Key;
                var otherYrBudget = entry.Value;

                Dictionary<string, BudgetPrefix> myBudgetPrefixDictionary = new Dictionary<string, BudgetPrefix>();
                Budget.Add(yr, myBudgetPrefixDictionary);

                foreach (var otherPrefix in otherYrBudget)
                {
                    string prefixKey = otherPrefix.Key;
                    BudgetPrefix prefix = otherPrefix.Value;

                    BudgetPrefix myPrefix = new BudgetPrefix
                    {
                        AssetType = prefix.AssetType,
                        District = prefix.District,
                        IsInterstate = prefix.IsInterstate,
                        Cost = zeroOut? 0.0 : prefix.Cost
                    };

                    myBudgetPrefixDictionary.Add(prefixKey, myPrefix);

                    foreach(var attributeCost in prefix.AttributeCosts)
                    {
                        Dictionary<string, double> myAttrCosts = new Dictionary<string, double>();
                        myPrefix.AttributeCosts.Add(attributeCost.Key, myAttrCosts);
                        foreach(var pair in attributeCost.Value)
                        {
                            myAttrCosts[pair.Key] = zeroOut ? 0.0 : pair.Value;
                        }
                    }
                }
            }
        }

        public void ZeroOut(MultiYearBudget other)
        {
            Copy(other, true);
        }

        public void Add(MultiYearBudget other)
        {
            Budget.Clear();
            foreach (var entry in other.Budget)
            {
                int yr = entry.Key;
                var otherYrBudget = entry.Value;

                if (Budget.ContainsKey(yr))
                {
                    Operate(yr, otherYrBudget, +1);
                }
            }
        }

        public void Subtract(MultiYearBudget other)
        {
            Budget.Clear();
            foreach (var entry in other.Budget)
            {
                int yr = entry.Key;
                var otherYrBudget = entry.Value;

                if (Budget.ContainsKey(yr))
                {
                    Operate(yr, otherYrBudget, -1);
                }
            }
        }

        private void Operate(int yr, Dictionary<string, BudgetPrefix> otherYrBudget, int mode)
        {
            var myYrBudget = Budget[yr];

            foreach (var otherPrefix in otherYrBudget)
            {
                string prefixKey = otherPrefix.Key;

                if (myYrBudget.ContainsKey(prefixKey))
                {
                    BudgetPrefix prefix = otherPrefix.Value;
                    BudgetPrefix myPrefix = myYrBudget[prefixKey];

                    if (mode ==0)
                    {
                        myPrefix.Cost = 0;
                    }
                    else if (mode==1) 
                    {
                        myPrefix.Cost += prefix.Cost;
                    }
                    else
                    {
                        myPrefix.Cost -= prefix.Cost;
                    }

                    foreach (var otherAttrNameVar in prefix.AttributeCosts)
                    {
                        var otherAttrName = otherAttrNameVar.Key;
                        var otherAttrCosts = otherAttrNameVar.Value;

                        if (myPrefix.AttributeCosts.ContainsKey(otherAttrName))
                        {
                            var myAttributeCosts = myPrefix.AttributeCosts[otherAttrName];
                            foreach (var otherAttrCostsVal in otherAttrCosts)
                            {
                                string key = otherAttrCostsVal.Key;
                                if (myAttributeCosts.ContainsKey(key))
                                {
                                    if (mode == 0)
                                    {
                                        myAttributeCosts[key] = 0;
                                    }
                                    else if (mode == 1)
                                    {
                                        myAttributeCosts[key] += otherAttrCostsVal.Value;
                                    }
                                    else
                                    {
                                        myAttributeCosts[key] -= otherAttrCostsVal.Value;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
