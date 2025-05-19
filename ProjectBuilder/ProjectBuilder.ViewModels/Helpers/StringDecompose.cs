using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.ViewModels
{
    internal static class StringDecompose
    {
        public static T[] DecomposeString<T>(this string str,char spliter)
        {
            try
            {
              if (string.IsNullOrEmpty(str))
                   return Array.Empty<T>();
              var values = str.Split(spliter);
              return values.Select(v => 
              {
                  if (string.IsNullOrEmpty(v))
                      return default(T);
                 return Convert.ChangeType(v, typeof(T));
              }).OfType<T>().ToArray();  
                          
            }
            catch
            {
                return Array.Empty<T>();
            }
        }
    }
}
