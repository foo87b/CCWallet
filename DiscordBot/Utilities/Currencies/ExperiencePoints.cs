using CCWallet.DiscordBot.Altcoins;
using NBitcoin;

namespace CCWallet.DiscordBot.Utilities.Currencies
{
    public class ExperiencePoints : CurrencyBase
    {
        public const string NetworkName = "xp-main";
        public override string Name { get; } = "eXperience Points";
        public override string Symbol { get; } = "XP";
        public override string BalanceFormatString { get; } = "N6";

        public ExperiencePoints() : base(NetworkName)
        {
        }

        protected override Network BuildAndRegisterNetwork()
        {
            return XPCoin.Instance.Mainnet;
        }
    }
}
