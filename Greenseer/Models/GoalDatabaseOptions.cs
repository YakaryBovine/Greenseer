namespace Greenseer.Models;

public sealed class GoalDatabaseOptions
{
  public const string GoalDatabase = "GoalDatabase";
  
  public string ConnectionString { get; set; } = null!;

  public string DatabaseName { get; set; } = null!;

  public string DatabaseVersionCollectionName { get; set; } = null!;
  
  public string GoalsCollectionName { get; set; } = null!;
  
  public string PlayerCollectionName { get; set; } = null!;

  public string SessionCollectionName { get; set; } = null!;

  public string GlobalSettingsCollectionName { get; set; } = null!;
}