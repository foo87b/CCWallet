using CCWallet.DiscordBot.Utilities;
using NBitcoin;
using NBitcoin.Protocol;
using System;
using System.Globalization;
using System.IO;

namespace CCWallet.DiscordBot.Currencies
{
    public class XPCoin : NetworkSetBase, ICurrency
    {
        public static XPCoin Instance { get; } = new XPCoin();

        public override string CryptoCode { get; } = "XP";
        string ICurrency.Name { get; } = "eXperience Points";
        string ICurrency.IconUrl { get; } = "https://raw.githubusercontent.com/eXperiencePoints/XPCoin/master/src/qt/res/icons/bitcoin.png";
        int ICurrency.BIP44CoinType { get; } = 0x70000001;
        int ICurrency.TransactionConfirms { get; } = 6;

        private XPCoin()
        {
        }

        public class XPCoinConsensusFactory : ConsensusFactory
        {
            public static XPCoinConsensusFactory Instance { get; } = new XPCoinConsensusFactory();

            private XPCoinConsensusFactory()
            {
            }

            public override Transaction CreateTransaction()
            {
                return new XPCoinTransaction();
            }
        }

        public class XPCoinTransaction : Transaction
        {
            private UInt32 nTime = 0;

            public DateTimeOffset Time
            {
                get { return DateTimeOffset.FromUnixTimeSeconds(nTime); }
                set { nTime = Convert.ToUInt32(value.ToUnixTimeSeconds()); }
            }

            public override ConsensusFactory GetConsensusFactory()
            {
                return Instance.Mainnet.Consensus.ConsensusFactory;
            }

            public override void ReadWrite(BitcoinStream stream)
            {
                using (var memory = new MemoryStream())
                {
                    var tx = new BitcoinStream(memory, stream.Serializing)
                    {
                        Type = stream.Type,
                        ProtocolVersion = stream.ProtocolVersion,
                        TransactionOptions = stream.TransactionOptions,
                    };

                    if (!stream.Serializing)
                    {
                        var header = stream.Inner.ReadBytes(8);
                        memory.Write(header, 0, 4);
                        stream.Inner.CopyTo(memory);

                        memory.Position = 0;
                        base.ReadWrite(tx);
                        nTime = BitConverter.ToUInt32(header, 4);
                    }
                    else
                    {
                        base.ReadWrite(tx);

                        var binary = memory.ToArray();
                        stream.ReadWrite(ref binary, 0, 4);                 // int nVersion
                        stream.ReadWrite(ref nTime);                        // uint32_t nTime
                        stream.ReadWrite(ref binary, 4, binary.Length - 4); // std::vector<CTxIn> vin; std::vector<CTxOut> vout; uint32_t nLockTime;
                    }
                }
            }
        }

        protected override NetworkBuilder CreateMainnet()
        {
            return new NetworkBuilder()
                .SetConsensus(new Consensus()
                {
                    ConsensusFactory = XPCoinConsensusFactory.Instance,
                    SupportSegwit = false,
                })
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 75 })
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 20 })
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 203 })
                .SetMagic(0xe5e2f8b4)
                .SetPort(28192)
                .SetRPCPort(28191)
                .SetName("xp-main")
                .AddAlias("xp-mainnet")
                .AddAlias("xpcoin-main")
                .AddAlias("xpcoin-mainnet")
                .AddDNSSeeds(new[]
                {
                    new DNSSeedData("seed1.xpcoin.io", "seed1.xpcoin.io"),
                    new DNSSeedData("seed2.xpcoin.io", "seed2.xpcoin.io"),
                    new DNSSeedData("seed3.xpcoin.io", "seed3.xpcoin.io"),
                    new DNSSeedData("seed4.xpcoin.io", "seed4.xpcoin.io"),
                })
                .AddSeeds(new NetworkAddress[0])
                .SetGenesis("01000000000000000000000000000000000000000000000000000000000000000000000001c0ee41fb6b792f9d4ad6a295812747aebc30972c326cfed6b9a62f27c283ccb808bd57ffff0f1ee9fb03000101000000b508bd57010000000000000000000000000000000000000000000000000000000000000000ffffffff1304ffff001d020f270a58502047656e65736973ffffffff010000000000000000000000000000");
        }

        protected override NetworkBuilder CreateTestnet()
        {
            return new NetworkBuilder()
                .SetConsensus(new Consensus()
                {
                    ConsensusFactory = XPCoinConsensusFactory.Instance,
                    SupportSegwit = false,
                })
                .SetBase58Bytes(Base58Type.PUBKEY_ADDRESS, new byte[] { 111 })
                .SetBase58Bytes(Base58Type.SCRIPT_ADDRESS, new byte[] { 196 })
                .SetBase58Bytes(Base58Type.SECRET_KEY, new byte[] { 239 })
                .SetMagic(0xefc0f2cd)
                .SetPort(17778)
                .SetRPCPort(18345)
                .SetName("xp-test")
                .AddAlias("xp-testnet")
                .AddAlias("xpcoin-test")
                .AddAlias("xpcoin-testnet")
                .AddSeeds(new NetworkAddress[0]);
        }

        protected override NetworkBuilder CreateRegtest()
        {
            // The currency side does not implement it.
            return new NetworkBuilder()
                .SetConsensus(new Consensus())
                .SetName("xp-reg")
                .AddAlias("xp-regtest")
                .AddAlias("xpcoin-reg")
                .AddAlias("xpcoin-regtest");
        }

        string ICurrency.FormatBalance(Money money, CultureInfo culture, bool symbol)
        {
            return money.ToDecimal(MoneyUnit.BTC).ToString("N6", culture) + (symbol ? $" {CryptoCode}" : string.Empty);
        }
    }
}
