using CCWallet.DiscordBot.Utilities.Currencies;
using Discord;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Utilities
{
    public class UserWallet
    {
        public IUser User { get; }
        public BitcoinAddress Address { get; }
        public string TotalBalance => FormatBalance(PendingMoney + ConfirmedMoney);
        public string PendingBalance => FormatBalance(PendingMoney);
        public string ConfirmedBalance => FormatBalance(ConfirmedMoney);
        public string UnconfirmedBalance => FormatBalance(UnconfirmedMoney);

        private ExtKey ExtKey { get; }
        private CurrencyBase Currency { get; }
        private List<Coin> UnspentCoins { get; } = new List<Coin>();
        private Money PendingMoney { get; set; } = Money.Zero;
        private Money ConfirmedMoney { get; set; } = Money.Zero;
        private Money UnconfirmedMoney { get; set; } = Money.Zero;

        internal UserWallet(CurrencyBase currency, IUser user, ExtKey key)
        {
            User = user;
            ExtKey = key;
            Currency = currency;

            Address = GetExtKey().ScriptPubKey.GetDestinationAddress(Currency.Network);
        }

        public async Task UpdateBalanceAsync()
        {
            var result = await Currency.Insight.GetUnspentCoinsAsync(Address);
            var require = (ulong) Currency.RequireConfirms;
            var pending = new List<Coin>();
            var confirmed = new List<Coin>();
            var unconfirmed = new List<Coin>();

            foreach ((var coin, var confirms) in result)
            {
                if (confirms <= 0)
                {
                    unconfirmed.Add(coin);
                }
                else if (confirms < require)
                {
                    pending.Add(coin);
                }
                else
                {
                    confirmed.Add(coin);
                }
            }

            UnspentCoins.Clear();
            UnspentCoins.AddRange(confirmed);
            
            PendingMoney = MoneyExtensions.Sum(pending.Select(c => c.Amount));
            ConfirmedMoney = MoneyExtensions.Sum(confirmed.Select(c => c.Amount));
            UnconfirmedMoney = MoneyExtensions.Sum(unconfirmed.Select(c => c.Amount));
        }

        private ExtKey GetExtKey(int account = 0, int change = 0, int index = 0)
        {
            // BIP32 path: m / purpose' / coin_type' / account' / change / address_index
            return ExtKey.Derive(GetKeyPath(type: GetCoinType(), account: account, change: change, index: index));
        }

        private KeyPath GetKeyPath(int purpose = 44, int type = 0, int account = 0, int change = 0, int index = 0)
        {
            // BIP-0044: Multi-Account Hierarchy for Deterministic Wallets
            return new KeyPath(new uint[]
            {
                0x80000000 | Convert.ToUInt32(purpose),
                0x80000000 | Convert.ToUInt32(type),
                0x80000000 | Convert.ToUInt32(account),
                0x7FFFFFFF & Convert.ToUInt32(change),
                0x7FFFFFFF & Convert.ToUInt32(index),
            });
        }

        private int GetCoinType()
        {
            // SLIP-0044: Registered coin types for BIP-0044
            // if not implementing BIP-0044 currency, use numbers above 0x70000000
            switch (Currency.Network.Name)
            {
                case ExperiencePoints.NetworkName:
                    return 0x70000001;

                default:
                    if (Currency.Network.NetworkType == NetworkType.Mainnet)
                    {
                        throw new ArgumentException();
                    }

                    return 0x00000001;
            }
        }

        private string FormatBalance(Money money)
        {
            return  money.ToDecimal(MoneyUnit.BTC).ToString(Currency.BalanceFormatString);
        }
    }
}
