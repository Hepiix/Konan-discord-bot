﻿using DiscordBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class ConfigBotService : InteractionModuleBase<SocketInteractionContext>
    {
        List<ConfigModel> _configModels;
        public ConfigBotService()
        {
            using (var s = new StreamReader("JsonFiles/configbot.json"))
            {
                var jsonString = s.ReadToEnd();
                try
                {
                    _configModels = JsonConvert.DeserializeObject<List<ConfigModel>>(jsonString) ?? throw new Exception();
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to read configbot.json! \n" + ex.Message);
                }

                if (_configModels == null)
                    throw new Exception("Failed to read configbot.json");
            }
        }

        public IReadOnlyCollection<SocketGuildChannel> GetAllChannels(SocketInteractionContext context)
        {
            IReadOnlyCollection<SocketGuildChannel> channels = new List<SocketGuildChannel>();
            channels = context.Guild.Channels;
            
            return channels;
        }

        public ComponentBuilder Build(SocketInteractionContext context)
        {
            var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wybierz kanał na którym mają być wysyłane urodziny postaci z anime")
            .WithCustomId("configMenu")
            .WithMinValues(1)
            .WithMaxValues(1);

            IReadOnlyCollection<SocketGuildChannel> guildChannels = GetAllChannels(context);

            foreach (SocketGuildChannel channel in guildChannels)
            {
                if (channel.GetChannelType() is ChannelType.Text)
                {
                    menuBuilder.AddOption($"{channel.Name}", $"{channel.Id}", " ");
                }
            }
            menuBuilder.AddOption("Wyłącz", "0", "Wyłącza funkcje");
            var builder = new ComponentBuilder().WithSelectMenu(menuBuilder);
            return builder;
        }



        public void SaveConfig(ulong? birthdayChannelId, ulong? guildId)
        {
            List<ConfigModel> configList = new List<ConfigModel>();
            configList = _configModels;
            Console.WriteLine(guildId);
            ConfigModel config = new ConfigModel();
            config = CheckIfGuildIsInConfig(guildId);
            if (config == null)
            {
                config = new ConfigModel();
                config.guildId = (ulong)guildId;
                config.birthdayChannelId = (ulong)birthdayChannelId;
                configList.Add(config);
            }
            else
            {
                config.birthdayChannelId = (ulong)birthdayChannelId;
            }
            
            
            
            
            


            string jsonf = JsonConvert.SerializeObject(configList.ToArray());
            System.IO.File.WriteAllText("JsonFiles/configbot.json", jsonf);
        }

        public ConfigModel? CheckIfGuildIsInConfig(ulong? guildId)
            => _configModels.Where(x => x.guildId == guildId).FirstOrDefault();
    }
}
