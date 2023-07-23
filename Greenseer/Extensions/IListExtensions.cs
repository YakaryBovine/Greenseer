namespace Greenseer.Extensions;

public static class ListExtensions
{
  private static readonly Random Random = new();
  
  public static T GetRandom<T>(this IList<T> list) => list[Random.Next(0, list.Count - 1)];
}