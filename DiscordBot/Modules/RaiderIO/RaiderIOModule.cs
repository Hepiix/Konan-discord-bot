using Discord.Commands;
using DiscordBot.Modules.RaiderIO.Models;

namespace DiscordBot.Modules.RaiderIO
{
    public class RaiderIOModule : InteractionModuleBase<SocketInteractionContext>
    {
        public RaiderIOService RaiderIOService { get; set; }

        [SlashCommand("score", "Pokaż score danej postaci")]
        public async Task ScoreSend([Name("Nazwapostaci")] [MinLength(2)]string charName, [Name("Nazwaserwera")] [MinLength(4)]string realmName)
        {
            realmName.Replace(' ', '-').ToLower();
            CharacterRIO character = await RaiderIOService.GetData(charName, realmName);
            if (character == null)
            {
                await RespondAsync("Spróbuj ponownie później!");
            }
            else
            {
                await RespondAsync($"{character.Name} - {character.MythicPlusScoresBySeason[0].Scores.All}");
            }
        }
    }
}
