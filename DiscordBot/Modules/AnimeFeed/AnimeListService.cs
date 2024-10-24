﻿using DiscordBot.Modules.AnimeFeed.Models;
using DiscordBot.Modules.Config.Models;
using DiscordBot.Modules.Config;
using DiscordBot.Services;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DiscordBot.Modules.AnimeFeed;

public class AnimeListService
{
    private readonly DiscordChatService _chatService;
    private readonly ConfigBotService _configBotService;
    private const string _dataRoot = "data";
    private const string _modulePath = "animeFeed";
    // 0 - guildId
    private const string _animeListJson = "animeList.json";
    private readonly Dictionary<ulong, List<Anime>> _animeList;

    private bool _contentChanged = false;

    public AnimeListService(
            DiscordChatService chatService,
            ConfigBotService configBotService)
    {
        _chatService = chatService;
        _configBotService = configBotService;
        string[] registeredGuilds;
        _animeList = [];

        try
        {
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }
        catch (DirectoryNotFoundException)
        {
            Directory.CreateDirectory(_dataRoot);
            registeredGuilds = Directory.GetDirectories(_dataRoot);
        }

        foreach (var registeredGuild in registeredGuilds)
        {
            string trimmedID = Regex.Replace(registeredGuild, "[^0-9]", "");
            _animeList.Add(ulong.Parse(trimmedID), []);
        }
        LoadFromJson();
    }

    public async Task UpdateAnimeList(IEnumerable<Anime> animeList)
    {
        foreach(var guild in _animeList) 
        {
            ulong weebChannelId = 0;
            foreach (ConfigModel model in _configBotService.GetConfigModels().Where(x => x.BirthdayChannelId != 0 && x.GuildId == guild.Key))
            {
                weebChannelId = model.BirthdayChannelId;
            }

            foreach (var guildAnime in guild.Value) 
            {
                var anime = animeList.Where(x=> x.Equals(guildAnime)).FirstOrDefault();

                if (anime is null || guildAnime.Episode == anime.Episode)
                    continue;

                guildAnime.Episode = anime.Episode;
                guildAnime.Url = anime.Url;
                guildAnime.Id = anime.Id;

                if ((guildAnime.Subscribers ?? []).Count > 0)
                    await _chatService.SendMessage(weebChannelId, guildAnime.GetUpdateMessage());
                _contentChanged = true;
            }
        }

        if(_contentChanged)
            SynchronizeJson();
    }

    public void AddAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        List<Anime> guildAnimeList;
        try
        {
            guildAnimeList = _animeList[guildId];
        }
        catch (KeyNotFoundException)
        {
            AddNewGuild(guildId);
            guildAnimeList = _animeList[guildId];
        }

        guildAnimeList??= [];

        var listEntry = guildAnimeList.Where(x=> x.Equals(anime)).FirstOrDefault();

        if (listEntry is null)
        {
            _animeList[guildId].Add(anime);
            listEntry = anime;
        }

        listEntry.Subscribers ??= [];

        // check if user is already on the list
        if ( !listEntry.Subscribers.Contains(userId))
        {
            listEntry.Subscribers.Add(userId);
            _contentChanged = true;
        }
    }

    public void RemoveAnimeSubscriber(ulong guildId, ulong userId, Anime anime)
    {
        var listEntry = _animeList[guildId].Where(x => x.Equals(anime)).FirstOrDefault();
        if (listEntry is null || listEntry.Subscribers is null || !listEntry.Subscribers.Contains(userId))
            return;

        listEntry.Subscribers.Remove(userId);
        _contentChanged = true;
    }

    private async void LoadFromJson()
    {
        Console.WriteLine("Loading AnimeFeed.json...");
        foreach (var anime in _animeList)
        {
            var filePath = Path.Combine(_dataRoot, anime.Key.ToString(), _modulePath, _animeListJson);
            string json;

            if (File.Exists(filePath) == false)
                return;

            using (var stream = new StreamReader(filePath))
            {
                try
                {
                    json = await stream.ReadToEndAsync();
                }
                catch (FileNotFoundException)
                {
                    json = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(json))
            {
                _animeList[anime.Key] ??= [];
                continue;
            }

            _animeList[anime.Key] = JsonConvert.DeserializeObject<List<Anime>>(json)!;
        }
    }

    private void SynchronizeJson()
    {
        Console.WriteLine("Saving AnimeFeed.json...");
        foreach (var guildAnimeList in _animeList)
        {
            var path = Path.Combine(_dataRoot,guildAnimeList.Key.ToString(),_modulePath);
            var filePath = Path.Combine(path,_animeListJson);
            string json = JsonConvert.SerializeObject(guildAnimeList.Value,Formatting.Indented);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            using var stream = new StreamWriter(new FileStream(filePath, FileMode.Create));
            stream.Write(json);
        }
        _contentChanged = false;
    }

    private void AddNewGuild(ulong guildId)
        => _animeList.Add(guildId, []);

    public IEnumerable<string?> GetUserAnimeList(ulong guildId, ulong userId)
    {
        List<Anime> guildAnimeList;
        try
        {
            guildAnimeList = _animeList[guildId];
            if (guildAnimeList is null || guildAnimeList.Count == 0)
                throw new Exception();
        }
        catch (KeyNotFoundException)
        {
            throw new Exception("Ty Pajacu! Nie obserwujesz żadnego anime!");
        }

        var userAnimeList = guildAnimeList.Where(x => x.Subscribers is not null && x.Subscribers.Contains(userId))
                                          .Select(x => x.Name)
                                          .ToList()
                                          .DefaultIfEmpty();

        return userAnimeList is null 
            ? throw new Exception("Ty Pajacu! Nie obserwujesz żadnego anime!")
            : userAnimeList;
    }
}
