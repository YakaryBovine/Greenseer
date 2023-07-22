using Discord;
using Discord.Interactions;

namespace Greenseer.TypeConverters;

public class NullTypeConverter<T> : TypeConverter<T>
{
  public override ApplicationCommandOptionType GetDiscordType()
  {
    return ApplicationCommandOptionType.String;
  }

  public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
  {
    return Task.FromResult(TypeConverterResult.FromSuccess(true));
  }
}