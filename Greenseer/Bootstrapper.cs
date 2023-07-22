using Microsoft.Extensions.DependencyInjection;

namespace Greenseer;

public static class Bootstrapper
{
  public static IServiceProvider ServiceProvider
  {
    get
    {
      if (_serviceProvider == null)
        throw new InvalidOperationException($"{nameof(Bootstrapper)} has not been initialized. Call {nameof(InitializeServiceProvider)} first.");
      return _serviceProvider;
    }
    private set => _serviceProvider = value;
  }

  public static readonly IServiceCollection ServiceCollection = new ServiceCollection();
  
  private static IServiceProvider? _serviceProvider;

  public static void InitializeServiceProvider()
  {
    ServiceProvider = ServiceCollection
      .BuildServiceProvider();
  }
}