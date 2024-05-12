using DiscordBot.Modules.Tarot.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using DiscordBot.Models;
using DiscordBot.Services;
using System.Threading.Channels;
using DiscordBot.Modules.RaiderIO.Models;

namespace DiscordBot.Modules.RaiderIO
{
    public class RaiderIOService : InteractionModuleBase<SocketInteractionContext>
    {
        HttpClient _httpClient = new HttpClient();
        public async Task<CharacterRIO> GetData(string charName, string realmName)
        {
            string apiLink = $"https://raider.io/api/v1/characters/profile?region=eu&realm={realmName}&name={charName}&fields=guild%2Cgear%2Craid_progression%2Cmythic_plus_scores_by_season%3Acurrent";
            HttpResponseMessage response = await _httpClient.GetAsync(apiLink);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                CharacterRIO character = JsonConvert.DeserializeObject<CharacterRIO>(responseBody);
                GenerateImage(character);
                return character;
            }
            else
            {
                return null;
            }
        }

        private void GenerateImage(CharacterRIO character)
        {

        }
    }
}
