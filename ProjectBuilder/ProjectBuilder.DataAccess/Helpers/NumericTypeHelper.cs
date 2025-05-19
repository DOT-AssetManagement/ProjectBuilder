using System;
using System.Collections.Generic;

namespace ProjectBuilder.DataAccess
{
    public static class NumericTypeHelper
    {
        private static readonly HashSet<Type> _numericTypes = new HashSet<Type>() 
        { 
            typeof (int),
            typeof (long),
            typeof (float),
            typeof (double),
            typeof (byte),
            typeof (decimal)
        };
        public static bool IsNumericType(this Type type)
        {
            return _numericTypes.Contains(type);    
        }
        public static bool IsBoolean(this Type type)
        {
            return type == typeof(bool);
        }
        public static bool IsString(this Type type)
        {
            return type == typeof(string);
        }
    }
}
