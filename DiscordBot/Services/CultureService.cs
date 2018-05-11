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

        private Dictionary<string, Catalog> Translations { get; } = new Dictionary<string, Catalog>();
        private Dictionary<ulong, string> GuildLanguages { get; } = new Dictionary<ulong, string>();
        private Dictionary<ulong, string> GroupLanguages { get; } = new Dictionary<ulong, string>();
        private Dictionary<ulong, string> UserLanguages { get; } = new Dictionary<ulong, string>();

        public CultureService()
        {
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

        public void SetLanguage(IMessageChannel channel, string lang)
        {
            var culture = GetCatalog(new CultureInfo(lang)).CultureInfo;
            
            switch (channel)
            {
                case IDMChannel dm:
                    UserLanguages[dm.Recipient.Id] = culture.Name;
                    break;

                case IGroupChannel group:
                    GroupLanguages[group.Id] = culture.Name;
                    break;

                case IGuildChannel guild:
                    GuildLanguages[guild.GuildId] = culture.Name;
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
                        return GetCatalog(UserLanguages[dm.Recipient.Id]);

                    case IGroupChannel group:
                        return GetCatalog(GroupLanguages[group.Id]);

                    case IGuildChannel guild:
                        return GetCatalog(GuildLanguages[guild.GuildId]);

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
