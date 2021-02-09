namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyResolvingProviderExtensions
    {
        public static IServiceCollection AddDependencyResolvingProvider(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(typeof(IDependencyResolvingProvider<>), typeof(DependencyResolvingProvider<>));
            return serviceCollection;
        }
    }
}
