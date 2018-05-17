using CCWallet.DiscordBot.Services;
using Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CCWallet.DiscordBot.Utilities.Preference
{
    public class Guild : AWSLambda.Entities.GuildPreference
    {
        public bool Changed { get; private set; } = false;
        public static PreferenceService PreferenceService { get; set; }

        private Dictionary<ulong, CultureInfo> CultureInfoList { get; set; } = new Dictionary<ulong, CultureInfo>();

        public override Dictionary<string, string> Languages
        {
            get => CultureInfoList.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value.Name);
            set => CultureInfoList = value.ToDictionary(kv => Convert.ToUInt64(kv.Key), kv => new CultureInfo(kv.Value));
        }
        
        public CultureInfo GetCaltureInfo(IMessageChannel channel = null)
        {
            return CultureInfoList.GetValueOrDefault(channel?.Id ?? 0, CultureInfoList.GetValueOrDefault(0UL));
        }

        public void SetCultureInfo(CultureInfo culture, IUserMessage message, ulong channel = 0)
        {
            CultureInfoList[channel] = culture;
            LastUpdate = message.Id;
            Changed = true;

            PreferenceService.Update(this);
        }

        public void SetLanguage(string lang, IUserMessage message, ulong channel = 0)
        {
            SetCultureInfo(new CultureInfo(lang), message, channel);
        }
    }
}
