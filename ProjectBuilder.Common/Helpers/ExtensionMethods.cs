using Microsoft.Extensions.DependencyInjection;
using ProjectBuilder.ViewModels;
using System.Reflection;
using System.Text.Json;

//[assembly: InternalsVisibleTo("ProjectBuilder.Tests")]
namespace ProjectBuilder.Common.Helpers
{
    internal static class ExtensionMethods
    {
        /// <summary>
        /// registers the viewModels based on naming conventions the default end is "ViewModel" 
        /// </summary>
        /// <param name="serviceCollection">the service collection that holds the services</param>
        /// <param name="endsWith">the part which the class name ends with</param>
        /// <returns>the service collection that holds the services after registring the viewModels</returns>
        public static IServiceCollection RegisterViewModels(this IServiceCollection serviceCollection, string endsWith = "ViewModel")
        {
            var assembly = Assembly.GetAssembly(typeof(ShellViewModel));
            if (assembly is not null)
                assembly.GetTypes().Where(type => type.IsClass && type.Name.EndsWith(endsWith))
                        .ToList().ForEach(type => serviceCollection.AddTransient(type));
            return serviceCollection;
        }
        /// <summary>
        /// registers the views based on naming conventions the default end is "View"
        /// </summary>
        /// <param name="serviceCollection">the service collection that holds the services</param>
        /// <param name="endsWith">the part which the class name ends with</param>
        /// <returns>the service collection that holds the services after registring the views</returns>
        public static IServiceCollection RegisterViews(this IServiceCollection serviceCollection, string endsWith = "View")
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is not null)
                assembly.GetTypes().Where(type => type.IsClass && type.Name.EndsWith(endsWith))
                        .ToList().ForEach(type => serviceCollection.AddTransient(type));
            return serviceCollection;
        }
       
        /// <summary>
        /// cast an object to a specific type
        /// </summary>
        /// <typeparam name="T">the target type</typeparam>
        /// <param name="target">the object to be casted</param>
        /// <param name="castedValue">the casted value</param>
        /// <returns>true if the object has been casted otherwise false</returns>
        public static bool Cast<T>(this object target, out T castedValue)
        {
            castedValue = default;
            try
            {
                if (target is JsonElement jsonElement)
                {
                    castedValue = jsonElement.Deserialize<T>();
                    return true;
                }
                if (target is T value)
                {
                    castedValue = value;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// converts a <see cref="string"/> to a <see cref="Color"/>
        /// </summary>
        /// <param name="stringValue">the string to be converted</param>
        /// <param name="color">the resulted color</param>
        /// <returns>true if the string has been converted otherwise false</returns>
        //public static bool TryConvertToColorFromString(this string stringValue, out Color color)
        //{
        //    try
        //    {
        //        color = (Color)ColorConverter.ConvertFromString(stringValue);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
    }
}
