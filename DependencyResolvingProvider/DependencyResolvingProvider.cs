using System;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class DependencyResolvingProvider<TInterface> : IDependencyResolvingProvider<TInterface> where TInterface : class
    {
        public DependencyResolvingProvider(IServiceProvider serviceProvider)
        {
            // Call here to ensure we get an early error for an unregistered dependency instead of
            // delaying until the first usage.
            serviceProvider.GetRequiredService<TInterface>();

            Instance = ServiceProviderResolveWrapper<TInterface>.Create(serviceProvider);
        }

        public TInterface Instance { get; }
    }
}
