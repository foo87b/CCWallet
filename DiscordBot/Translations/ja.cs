using NGettext;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Translations
{
    public class ja : Catalog
    {
        public ja() : base(new CultureInfo("ja"))
        {
            Translations = new Dictionary<string, string[]>
            {
            };
        }
    }
}
