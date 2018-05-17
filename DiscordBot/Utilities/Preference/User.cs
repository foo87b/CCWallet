using CCWallet.DiscordBot.Services;
using Discord;
using System.Globalization;

namespace CCWallet.DiscordBot.Utilities.Preference
{
    public class User : AWSLambda.Entities.UserPreference
    {
        public bool Changed { get; private set; } = false;
        public CultureInfo CultureInfo { get; private set; }
        public static PreferenceService PreferenceService { get; set; }

        public override string Language
        {
            get => CultureInfo.Name;
            set => CultureInfo = new CultureInfo(value);
        }

        public void SetLanguage(string lang, IUserMessage message)
        {
            LastUpdate = message.Id;
            Language = lang;
            Changed = true;

            PreferenceService.Update(this);
        }
    }
}
