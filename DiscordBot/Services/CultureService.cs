using NGettext;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Services
{
    public class CultureService
    {
        private Dictionary<string, Catalog> Translations { get; } = new Dictionary<string, Catalog>();

        public CultureService()
        {
        }

        public void AddLocale<T>() where T : Catalog, new()
        {
            var catalog = new T();

            Translations[catalog.CultureInfo.Name] = catalog;
        }

        public Catalog GetCatalog(CultureInfo culture)
        {
            if (Translations.ContainsKey(culture.Name))
            {
                return Translations[culture.Name];
            }
            else if (Translations.ContainsKey(culture.Parent.Name))
            {
                return Translations[culture.Parent.Name];
            }
            else
            {
                return new Catalog();
            }
        }
    }
}
