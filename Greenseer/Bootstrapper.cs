using Microsoft.Extensions.DependencyInjection;

namespace Greenseer;

public static class Bootstrapper
{
  public static IServiceProvider ServiceProvider { get; set; }
  
  private static IServiceCollection _serviceCollection;
  private static bool _isInitialized;

  public static void Init()
  {
    if (_isInitialized) 
      return;
    
    var serviceCollection = new ServiceCollection();
    var serviceProvider = serviceCollection
      .BuildServiceProvider();

    _serviceCollection = serviceCollection;
    ServiceProvider = serviceProvider;
    _isInitialized = true;
  }
    
  public static void RegisterType<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
  {
    _serviceCollection.AddSingleton<TInterface, TImplementation>();
    ServiceProvider = _serviceCollection.BuildServiceProvider();
  }

  public static void RegisterInstance<TInterface>(TInterface instance)
    where TInterface : class
  {
    _serviceCollection.AddSingleton(instance);
    ServiceProvider = _serviceCollection.BuildServiceProvider();
  }
}