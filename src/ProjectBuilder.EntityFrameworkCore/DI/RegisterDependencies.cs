using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectBuilder.DataAccess;
using ProjectBuilder.Entities;
using ProjectBuilder.Models;
using ProjectBuilder.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace ProjectBuilder.DI
{
    public static class RegisterDependencies
    {
        public static IServiceCollection RegisterAutoMapper(this IServiceCollection services)
        {
            services.RegisterProfiles()
                    .AddSingleton(sr =>
                    {
                        var profiles = sr.GetServices<Profile>();
                        var config = new MapperConfiguration(cfg => cfg?.AddProfiles(profiles), NullLoggerFactory.Instance);
                        return config.CreateMapper();
                    });
            return services;
        }
        private static IServiceCollection RegisterProfiles(this IServiceCollection serviceCollection)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly is not null)
                assembly.GetTypes().Where(type => type.IsClass && type.Name.EndsWith("Profile"))
                                   .ToList()
                                   .ForEach(type => serviceCollection.AddTransient(typeof(Profile), type));
            return serviceCollection;
        }
        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddTransient<ICandidatePoolRepository, CandidatePoolRepository>();
            return services;
        }
    }
}
