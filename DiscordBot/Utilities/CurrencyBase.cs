using CCWallet.DiscordBot.Utilities.Insight;
using NBitcoin;
using System;

namespace CCWallet.DiscordBot.Utilities
{
    public abstract class CurrencyBase
    {
        public abstract string Name { get; }
        public abstract string Symbol { get; }
        public virtual int RequireConfirms { get; } = 6;
        public virtual string BalanceFormatString { get; } = "N8";
        public Network Network { get; protected set; }
        public InsightClient Insight { get; protected set; }

        protected CurrencyBase(string name)
        {
            Network = Network.GetNetwork(name) ?? BuildAndRegisterNetwork();
        }

        public virtual void SetupInsight(string endpoint)
        {
            if (Insight != null)
            {
                throw new InvalidOperationException();
            }

            Insight = new InsightClient(endpoint);
        }

        protected abstract Network BuildAndRegisterNetwork();
    }
}
