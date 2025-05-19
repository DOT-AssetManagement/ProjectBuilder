using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;

namespace ProjectBuilder.Tests
{
    internal class PrivateObjectHelper
    {
        internal static void InvokePrivateMethod(object target,string methodName,params object[] parameters) 
        {
            var methodInfo = target.GetType().GetMethod(methodName,BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy );
            methodInfo?.Invoke(target, parameters);
        }
        internal static void InvokePrivateGenericMethod<T>(object target, string methodName, params object[] parameters)
        {
            var type = typeof(T);
            var methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var targetMethod = methodInfo.MakeGenericMethod(type);
            targetMethod?.Invoke(target, parameters);
        }
        internal static T InvokePrivateMethod<T>(object target, string methodName, params object[] parameters)
        {
            var methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return (T)methodInfo?.Invoke(target, parameters);
        }
        internal static async Task InvokePrivateMethodAsync(object target, string methodName, params object[] parameters)
        {
            var methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            await (Task)methodInfo?.Invoke(target, parameters);
        }
        internal static async Task<T> InvokePrivateMethodAsync<T>(object target, string methodName, params object[] parameters)
        {
            var methodInfo = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return await (Task<T>)methodInfo?.Invoke(target, parameters);
        }
        internal static void InvokeEvent<TEventArgs>(object target,string eventName,TEventArgs eventArgs) where  TEventArgs: EventArgs
        {
                var eventDelegate = (MulticastDelegate)target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { target, eventArgs });
                    }
                }           
        }
        internal static List<T> LoadTestData<T>(int count) where T : new()
        {
            var list = new List<T>();   
            for (int i = 0; i < count; i++)
            {
               list.Add(new T());
            }
            return list;
        }
    }
}
