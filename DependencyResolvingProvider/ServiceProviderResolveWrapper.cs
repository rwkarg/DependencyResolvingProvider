using System;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ProxyBuilder")]
namespace Microsoft.Extensions.DependencyInjection
{
    internal class ServiceProviderResolveWrapper<T> : DispatchProxy where T : class
    {
#nullable disable
        private IServiceProvider _serviceProvider;
#nullable restore

        public static T Create(IServiceProvider serviceProvider)
        {
            _ = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));

            // Make the proxy that can be either a T or ServiceProviderResolveWrapper<T>
            object proxy = Create<T, ServiceProviderResolveWrapper<T>>();
            // Configure as ServiceProviderResolveWrapper with the service provider
            var proxyAsWrapper = (ServiceProviderResolveWrapper<T>)proxy;
            proxyAsWrapper._serviceProvider = serviceProvider;

            // Return it as a T
            return (T)proxy;
        }

        /// <summary>
        /// Resolves an instance of T for each method invocation
        /// </summary>
        /// <param name="targetMethod"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <remarks>This will respect how the T was registered.
        /// Instance will always resolve to a new instance while Scoped or Singleton will return the same instance in the same scope or program, respectively.</remarks>
        protected override object? Invoke(MethodInfo targetMethod, object[] args)
        {
            _ = targetMethod ?? throw new ArgumentException(nameof(targetMethod));

            // Resolve a new instance to invoke the method on
            var newInstance = _serviceProvider.GetRequiredService<T>();
            var result = targetMethod.Invoke(newInstance, args);
            return result;
        }
    }
}
