using Discord;
using NGettext;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Services
{
    public class CultureService
    {
        public CultureInfo DefaultCultureInfo { get; } = new CultureInfo("en");

        private PreferenceService Preference { get; }
        private Dictionary<string, Catalog> Translations { get; } = new Dictionary<string, Catalog>();

        public CultureService(PreferenceService preference)
        {
            Preference = preference;
        }

        public void AddTranslation<T>() where T : Catalog, new()
        {
            var catalog = new T();

            Translations[catalog.CultureInfo.Name] = catalog;
        }

        public bool HasTranslation(string lang)
        {
            return DefaultCultureInfo.Name == lang || (Translations.ContainsKey(lang) && Translations[lang].Translations.Count > 0);
        }

        public void SetLanguage(IMessageChannel channel, IUserMessage message, string lang, bool channelOnly = false)
        {
            var culture = GetCatalog(new CultureInfo(lang)).CultureInfo;
            
            switch (channel)
            {
                case IDMChannel dm:
                    Preference.GetUserPreference(dm.Recipient.Id).SetLanguage(lang, message);
                    break;

                case IGroupChannel group:
                    Preference.GetGroupPreference(group.Id).SetLanguage(lang, message);
                    break;

                case IGuildChannel guild:
                    Preference.GetGuildPreference(guild.GuildId).SetLanguage(lang, message, channelOnly ? channel.Id : 0);
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        public Catalog GetCatalog(IMessageChannel channel)
        {
            try
            {
                switch (channel)
                {
                    case IDMChannel dm:
                        return GetCatalog(Preference.GetUserPreference(dm.Recipient.Id).CultureInfo);

                    case IGroupChannel group:
                        return GetCatalog(Preference.GetGroupPreference(group.Id).CultureInfo);

                    case IGuildChannel guild:
                        return GetCatalog(Preference.GetGuildPreference(guild.GuildId).GetCaltureInfo(channel));

                    default:
                        throw new ArgumentException();
                }
            }
            catch (KeyNotFoundException)
            {
                return GetCatalog();
            }
        }

        public Catalog GetCatalog(string lang)
        {
            return GetCatalog(new CultureInfo(lang));
        }

        public Catalog GetCatalog(CultureInfo culture = null)
        {
            culture = culture ?? DefaultCultureInfo;
            
            return Translations.ContainsKey(culture.Name)
                ? Translations[culture.Name]
                : (Translations.ContainsKey(culture.Parent.Name) ? Translations[culture.Parent.Name] : CreateCatalog(culture));
        }

        private Catalog CreateCatalog(CultureInfo culture)
        {
            Translations.Add(culture.Name, new Catalog(culture));

            return Translations[culture.Name];
        }
    }
}
