﻿using DiscordBot.Modules.AntiSpam;
using DiscordBot.Modules.Tarot.Models;
using System.Security.Cryptography;

namespace DiscordBot.Modules.Tarot
{
    public class TarotCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        public TarotService TarotService { get; set; }
        public AntiSpamService AntiSpamService { get; set; }
        private readonly ILogger<CommandModule> _logger;

        private const long _schizoID = 792728812730449941;
        private const long _alcoholicID = 1192192605941932093;

        [SlashCommand("kartadnia", "Losuje karte dnia tarota")]
        public async Task TarotCard()
        {
            if (AntiSpamService._blockedUsers.Contains((long)Context.User.Id))
            {
                await RespondAsync($"Proszeee, zostaw mnie w spokoju, nyaa~! ✨💖🌸", ephemeral: true);
            }
            else
            {
                AntiSpamService._blockedUsers.Add((long)Context.User.Id);
                if (Context.Guild.Id == 606253518281768983)
                {
                    if (RandomNumberGenerator.GetInt32(1000) == 0)
                    {
                        Context.User.SendMessageAsync("Niestety przegrałeś/aś w rosyjskiej ruletce, oto twoja kulka w głowę\nhttps://cdn.discordapp.com/attachments/1036702964754165901/1242471907953999966/IMG_1507.png?ex=664df5a2&is=664ca422&hm=ef422c38d8edb1fec05f074f5222cd2223d8a36a35b391ab980dd5c4f54021cf&");
                        await RespondAsync($"Otrzymałeś/aś kulkę w głowę w rosyjskiej ruletce...");
                        return;
                    }
                }
                if (Context.User.Id == _schizoID)
                    await RespondAsync("Przepraszam ale nie mam schizofrenii i nie rozmawiam sama ze sobą");
                else if (Context.User.Id == _alcoholicID)
                    await RespondAsync("Przepraszam ale nie mam problemu z alkoholem, nie dam ci nic na kreske, nie mam żadnego benzo i proszę nie wysyłaj więcej zdjęć swojego przyrodzenia...");
                else
                    await SendTarotCard(Context);
            }   
        }

        public async Task SendTarotCard(SocketInteractionContext message)
        {
            TarotCardsUsed user = TarotService.CheckIfUserUsedCard(message.User.Id.ToString());
            TarotCard card = new TarotCard();
            IUserMessage botMessage = null;

            if (user != null)
                card = TarotService.GetCard(user.Card);
            else
                card = TarotService.GetRandomCard();

            if (user != null && user.UsedTime >= 1)
            {
                if (message.Channel is SocketGuildChannel guildChannel)
                {
                    ulong guild = guildChannel.Guild.Id;

                    foreach (BotMessageId item in user.BotMessagesId)
                    {
                        if (item.GuildId == guild)
                        {
                            await RespondAsync($"<@{user.Id}> https://discord.com/channels/{item.GuildId}/{item.ChannelId}/{item.MessageId}");
                            return;
                        }
                    }

                    string desc = String.Format("**{0}**{1}\r\n```{2}```",
                        card.Name,
                        String.IsNullOrEmpty(card.Quote) ? null : "\r\n> "+ card.Quote,
                        card.Description );
                    await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                    IUserMessage userx = await GetOriginalResponseAsync();
                    ulong guildId = Context.Guild.Id;
                    ulong channelId = Context.Channel.Id;

                    TarotService.SaveTimeTarotCardUsed(user.Id, userx.Id, guildId, channelId);
                }
            }
            else
            {
                string desc = String.Format("**{0}**{1}\r\n```{2}```",
                         card.Name,
                         String.IsNullOrEmpty(card.Quote) ? null : "\r\n> " + card.Quote,
                         card.Description);
                await RespondWithFileAsync(filePath: TarotService.GetRandomCardPhotoPath(card), text: desc);
                IUserMessage userx = await GetOriginalResponseAsync();

                ulong guildId = Context.Guild.Id;
                ulong channelId = Context.Channel.Id;
                ulong testId = userx.Id;
                TarotService.SaveCardToUser(message.User.Id.ToString(), card.Name, 1, testId, guildId, channelId);
            }
        }
    }
}