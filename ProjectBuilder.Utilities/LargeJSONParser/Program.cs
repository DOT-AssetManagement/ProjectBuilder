using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LargeJSONParser
{

    class Program
    {
        static bool NoTreatment = false;

        enum Jstatus
        {
            Unknown = 0,
            InitialConditionOfNetwork = 100,
            InitialSectionSummaries = 200,
            IniFacilityName = 210,
            IniValuePerNumericAttribute = 220,
            IniValuePerTextAttribute = 230,
            Years = 300,
            YYear,
            YCondition ,
            YBudgets,
            YDefCondGoals,
            YSectionTreatments,
            YSAppliedTreatment,
            YSTreatmentCause,
            YSTreatmentConsiderations,
            YSTreatmentFundingIgnoresSpendingLimit,
            YSTreatmentOptions,
            YSTreatmentRejections,
            YSTreatmentSchedulingCollisions,
            YSTreatmentStatus,
            YSFacilityName,
            YSSectionName,
            YSValNumeric,
            YSValText,
            YTargetGoals,
        }

        private static Jstatus GetStatus(StringBuilder sb, Jstatus currentStatus)
        {

            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                string s = sb.ToString().Trim();


                if (currentStatus == Jstatus.InitialSectionSummaries || currentStatus == Jstatus.IniValuePerTextAttribute)
                {
                    if (s.StartsWith("\"FacilityName\"") || s.StartsWith("\"SectionName\""))
                    {
                        return Jstatus.IniFacilityName;
                    }

                }

                if (currentStatus == Jstatus.IniFacilityName || currentStatus == Jstatus.IniValuePerTextAttribute)
                {
                    if (s.StartsWith("\"ValuePerNumericAttribute\""))
                    {
                        return Jstatus.IniValuePerNumericAttribute;
                    }
                }

                if (currentStatus == Jstatus.IniFacilityName || currentStatus == Jstatus.IniValuePerNumericAttribute)
                {
                    if (s.StartsWith("\"ValuePerTextAttribute\""))
                    {
                        return Jstatus.IniValuePerTextAttribute;
                    }
                }

                if (s.StartsWith("\"InitialConditionOfNetwork\""))
                {
                    return Jstatus.InitialConditionOfNetwork;
                }
                if (s.StartsWith("\"InitialSectionSummaries\""))
                {
                    return Jstatus.InitialSectionSummaries;
                }
                if (s.StartsWith("\"Years\""))
                {
                    return Jstatus.Years;
                }

                List<string> YSList = new List<string>()
                {
                    "AppliedTreatment",
                    "TreatmentCause",
                    "TreatmentConsiderations",
                    "TreatmentFundingIgnoresSpendingLimit",
                    "TreatmentOptions",
                    "TreatmentRejections",
                    "TreatmentSchedulingCollisions",
                    "TreatmentsStatus",
                    "FacilityName",
                    "SectionName",
                    "ValuePerNumericAttribute",
                    "ValuePerTextAttribute"
                };

                if (currentStatus == Jstatus.YSectionTreatments 
                        || (currentStatus >= Jstatus.YSAppliedTreatment && currentStatus <= Jstatus.YSValText)
                        || currentStatus == Jstatus.YTargetGoals
                        || currentStatus == Jstatus.YYear)
                {
                    if (s.StartsWith("\"Target"))
                    {
                        return Jstatus.YTargetGoals;
                    }

                    if (s.StartsWith("\"Year\""))
                    {
                        return Jstatus.YYear;
                    }
                }

                if (currentStatus == Jstatus.YSectionTreatments || (currentStatus >= Jstatus.YSAppliedTreatment && currentStatus <= Jstatus.YSValText))
                {
                    string[] ss = s.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
                    string x = ss[0].Trim().Substring(1, ss[0].Trim().Length - 2);
                    int i = YSList.IndexOf(x);
                    if (i < 0)
                    {
                        return currentStatus;
                    }

                    if (i == 0)
                    {
                        NoTreatment = false;
                        string t = ss[1].Trim().Substring(1, ss[1].Trim().Length - 2);
                        if (t.StartsWith("No Treatment"))
                        {
                            NoTreatment = true;
                        }
                    }
                    return Jstatus.YSAppliedTreatment + i;
                }

          

                if (currentStatus == Jstatus.Years ||
                    currentStatus == Jstatus.YBudgets ||
                    currentStatus == Jstatus.YCondition ||
                    currentStatus == Jstatus.YDefCondGoals ||
                    currentStatus == Jstatus.YSectionTreatments ||
                    currentStatus == Jstatus.YTargetGoals ||
                    currentStatus == Jstatus.YYear)
                {
                    if (s.StartsWith("\"Budgets\""))
                    {
                        return Jstatus.YBudgets;
                    }
                    if (s.StartsWith("\"ConditionOfNetwork"))
                    {
                        return Jstatus.YCondition;
                    }
                    if (s.StartsWith("\"Deficient"))
                    {
                        return Jstatus.YDefCondGoals;
                    }
                    if (s.StartsWith("\"Sections"))
                    {
                        return Jstatus.YSectionTreatments;
                    }
                    if (s.StartsWith("\"Target"))
                    {
                        return Jstatus.YTargetGoals;
                    }
                    if (s.StartsWith("\"Year\""))
                    {
                        return Jstatus.YYear;
                    }
                    return currentStatus;
                }
            }

            return currentStatus;
        }

        private static bool OkToWrite(StringBuilder sb, Jstatus currentStatus)
        {
            string[] relevantAttrributes = new string[] { "ValuePerNumericAttribute", "ValuePerTextAttribute", "SEGMENT_LENGTH", "WIDTH", "LANES", "DISTRICT", "CNTY", "SR" };

            if (currentStatus == Jstatus.YCondition
                || currentStatus == Jstatus.YDefCondGoals
                || currentStatus == Jstatus.YTargetGoals)
            {
                return false;
            }

            if (NoTreatment && (currentStatus==Jstatus.YSTreatmentCause 
                || currentStatus == Jstatus.YSTreatmentOptions
                || currentStatus == Jstatus.YSTreatmentFundingIgnoresSpendingLimit))
            {
                return false;
            }

            if (currentStatus == Jstatus.YSTreatmentConsiderations
                || currentStatus == Jstatus.YSTreatmentRejections
                || currentStatus == Jstatus.YSTreatmentSchedulingCollisions
                )
            {
                return false;
            }

           
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                string s = sb.ToString().Trim();

                if (currentStatus == Jstatus.YSValText && !s.StartsWith("\"ValuePerText"))
                {
                    return false;
                }

                if (currentStatus == Jstatus.YSValNumeric && !s.StartsWith("\"ValuePerNumeric"))
                {
                    return false;
                }

                if (currentStatus == Jstatus.InitialSectionSummaries || currentStatus == Jstatus.IniValuePerNumericAttribute
                   || currentStatus == Jstatus.IniValuePerTextAttribute)
                {
                    if (s.StartsWith("\"Years\""))
                    {
                        return true;
                    }

                }

                if (currentStatus == Jstatus.IniValuePerNumericAttribute || currentStatus == Jstatus.IniValuePerTextAttribute)
                {
                    string[] ss = s.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    s = ss[0].Substring(1, ss[0].Length - 2);
                    if (relevantAttrributes.Contains(s))
                    { 
                        return true;
                    }
                    return false;
                }

               
            } 

            return true;
        }

        static void Main(string[] args)
        {
            string sInput = args[0];
            string sOutput = args[1];
            //long N = 2000000000;
            long M = 2000000;

            long i = 0;
            StringBuilder sb = new StringBuilder();
            string s;
            int n = 0;
            int m = 0;

            Jstatus jstatus = Jstatus.Unknown;
            NoTreatment = false;

            char cPrev = (char)0;
            char cPrevWritten = (char)0;

            using (StreamReader sr = new StreamReader(sInput))
            {
                using (StreamWriter sw = new StreamWriter(sOutput))
                {
                 
                    while (sr.Peek() >= 0 && /*i < N &&*/  m<M)
                    {
                        char C = (char)(sr.Read());
                        if (C == '{' || C == '[')
                        {
                            s = new string('\t', n);
                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                jstatus = GetStatus(sb, jstatus);
                                if (OkToWrite(sb, jstatus))
                                {
                                    sw.Write(s);
                                    sw.WriteLine(sb.ToString());
                                    m++;
                                }
                                sb.Clear();
                            }
                            if (OkToWrite(sb, jstatus))
                            {
                                sw.Write(s);
                                sw.WriteLine(C);
                                cPrevWritten = C;
                            }
                            n++;
                        }
                        else if (C == ']' || C == '}')
                        {

                            if (jstatus == Jstatus.IniValuePerTextAttribute)
                            {
                                jstatus = Jstatus.InitialSectionSummaries;
                            }

                            s = new string('\t', n);
                            
                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                jstatus = GetStatus(sb, jstatus);
                                if (OkToWrite(sb, jstatus))
                                {
                                    sw.Write(s);
                                    sw.WriteLine(sb.ToString());
                                    m++;
                                }
                                sb.Clear();
                            }
         
                            n--;
                       
                            if (OkToWrite(sb, jstatus))
                            {
                                s = new string('\t', n);
                                sw.Write(s);
                                sw.WriteLine(C);
                                cPrevWritten = C;
                            }

                          
                        }
                        else if (C == ',')
                        {
                            s = new string('\t', n);
                            if (!string.IsNullOrEmpty(sb.ToString()))
                            {
                                jstatus = GetStatus(sb, jstatus);
                                if (OkToWrite(sb, jstatus))
                                {
                                    sw.Write(s);
                                    sw.WriteLine($"{sb}{C}");
                                    cPrevWritten = C;
                                    m++;
                                }
                            }
                            else if ((cPrevWritten == '}' || cPrevWritten == ']'))
                            {
                                sw.Write(s);
                                sw.WriteLine(C);
                                cPrevWritten = C;
                                m++;
                            }
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(C);
                        }

                        cPrev = C;
                        i++;
                    }

                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        jstatus = GetStatus(sb, jstatus);
                        if (OkToWrite(sb, jstatus))
                        {
                            s = new string('\t', n);
                            sw.Write(s);
                            sw.WriteLine(sb.ToString());
                        }
                        sb.Clear();
                    }

                }
            }



            Console.WriteLine($"Characters read: {i}");
            Console.WriteLine($"Non-bracket lines written: {m}");
        }
    }
}
