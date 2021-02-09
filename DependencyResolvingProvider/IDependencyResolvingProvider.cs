namespace Microsoft.Extensions.DependencyInjection
{
    public interface IDependencyResolvingProvider<out TInterface>
    {
        TInterface Instance { get; }
    }
}
