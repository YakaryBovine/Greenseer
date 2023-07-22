namespace Greenseer.Models;

public sealed class GoalDatabaseOptions
{
  public const string GoalDatabase = "GoalDatabase";
  
  public string ConnectionString { get; set; } = null!;

  public string DatabaseName { get; set; } = null!;

  public string BooksCollectionName { get; set; } = null!;
}