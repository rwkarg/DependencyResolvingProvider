# DependencyResolvingProvider
Inject dependencies that are resolved each time they are used instead of only when they are injected.


# Background
There are some legacy components that register themselves as a single instance that lives for the life of a program but require that dependencies be resolved more frequently than that.

One example is an IHostedService implementation that connects to a message queue and processes messages. If that processing requires making a call on an HttpClient, then that client should be created from an HttpClientFactory as needed; ideally as a Typed HTTP Client. But if the Typed HTTP Client is added as a dependency in the IHostedService's constructor then it will only be resolved once and never be re-resolved, leading to DNS freshness issues.

Ideally, there would be a way to register the Typed HTTP Client (or anything else that needs to be resolved per Scope or Usage/Instance) such that it gets re-resolved automatically.

Other approaches than what is presented here are rebuilding the IHostedService or other Singleton component to look more like an ASP.NET Core Controller where a separate instance of "something" is created and filled in by DI per request. This is not always possible due to lack of access to the original component or lack of time to make the conversion presently. Or the dependency could be registered in DI and injected as a Func<T> and be up to the consumer to do the right thing and only hold that Func<T> as a member variable and resolve the dependency from that at the right time. This avoids the Pit of Success by allowing the consuming code to (incorrectly) call the Func<T> once and caching the result, there by avoiding the re-resolution we are trying to achieve.

# Implementation
This code registeres a generic proxy that can be used around any registered type. The proxy contains an IServiceProvider and will resolve the underlying type from that provider on any call to the underlying type. ServiceProviderResolveWrapper is a DispatchProxy implementation that handles the proxying and holds the IServiceProvider instance.

IDependencyResolvingProvider<T> is what gets injected instead of a T in the consuming code's constructor.

DependencyResolvingProvider<T> handles creating the internal ServiceProviderResolveWrapper and exposing the Instance property that consuming code will use.

DependencyResovingProviderExtensions registers the open generic type that allows for any T to be specified for a DependencyResolvingProvider<>. This allows for one registration to enable proxying of any type registered with DI.

# Usage
In ConfigureServices:

```C#
    services.AddDependencyResolvingProvider();
```

In a dependency's constructor:

```C#
public class MyHostedService : LegacyHostedService
{
    private IMyTypedHttpClient _typedClient;

    public MyHostedService(DependencyResolvingProvider<IMyTypedHttpClient> typedClient)
    {
        _typedClient = typedClient.Instance;
    }

    public string SomeMethod()
    {
        return _typedClient.GetTheThing();
    }
}
```

In SomeMethod, the call to .GetTheThing() is actually being called on the DispatchProxy implementation (that just looks like an IMyTypedHttpClient) which under the hood will resolve an IMyTypedHttpClient from the IServiceProvider, call .GetTheThing() on that resolved instance, and return the result. Every call to the proxy will resolve a new instance from DI.

The above pattern caches the typedClient.Instance (the actual proxy). It is also possible to store the DependencyResolvingProvider<IMyTypedHttpClient> and access the .Instance property on each use. There is no difference in behavior other than an extra property access and more typing (the .Instance each time it is used). Storing the .Instance in a field is preferred, if just from a clarity perspective, but correct behavior is maintained in either method.
