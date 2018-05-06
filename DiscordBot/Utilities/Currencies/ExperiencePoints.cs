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
            var builder = new NetworkBuilder();
            builder.CopyFrom(Network.Main); // FIXME

            builder.SetName(NetworkName)
                .SetNetworkType(NetworkType.Mainnet)
                .SetPort(28192)
                .SetRPCPort(28191)
                .SetMagic(0xe5e2f8b4)
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] {203})
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] {75})
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] {20})
                .SetGenesis("01000000000000000000000000000000000000000000000000000000000000000000000001c0ee41fb6b792f9d4ad6a295812747aebc30972c326cfed6b9a62f27c283ccb808bd57ffff0f1ee9fb03000101000000b508bd57010000000000000000000000000000000000000000000000000000000000000000ffffffff1304ffff001d020f270a58502047656e65736973ffffffff010000000000000000000000000000")
                .SetConsensus(new Consensus()
                {
                    SupportSegwit = false,
                });

            return builder.BuildAndRegister();
        }
    }
}
