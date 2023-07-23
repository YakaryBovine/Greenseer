namespace Greenseer.Models;

public enum GoalType
{
  /// <summary>
  /// Personal Goals can be drawn from the deck, and once complete, they are discarded.
  /// </summary>
  Personal,
  
  /// <summary>
  /// Story Goals are like Personal Goals, but they can only be added by an admin.
  /// </summary>
  Story,
  
  /// <summary>
  /// Universal Goals can be completed by any player any number of times, without being drawn from the deck.
  /// </summary>
  Universal
}