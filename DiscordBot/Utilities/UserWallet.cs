using CCWallet.DiscordBot.Services;
using CCWallet.DiscordBot.Utilities.Insight;
using Discord;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Utilities
{
    public class UserWallet
    {
        public IUser User { get; }
        public Network Network { get; }
        public ICurrency Currency { get; }
        public InsightClient Insight { get; }
        public BitcoinAddress Address { get; }
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
        public string TotalBalance => Currency.FormatBalance(PendingMoney + ConfirmedMoney, Culture);
        public string PendingBalance => Currency.FormatBalance(PendingMoney, Culture);
        public string ConfirmedBalance => Currency.FormatBalance(ConfirmedMoney, Culture);
        public string UnconfirmedBalance => Currency.FormatBalance(UnconfirmedMoney, Culture);

        private ExtKey ExtKey { get; }
        private List<Coin> UnspentCoins { get; } = new List<Coin>();
        private Money PendingMoney { get; set; } = Money.Zero;
        private Money ConfirmedMoney { get; set; } = Money.Zero;
        private Money UnconfirmedMoney { get; set; } = Money.Zero;

        public UserWallet(WalletService wallet, Network network, IUser user, ExtKey key)
        {
            Network = network;
            Insight = wallet.GetInsightClient(network);
            Currency = wallet.GetCurrency(network);

            User = user;
            ExtKey = key;
            Address = GetExtKey().ScriptPubKey.GetDestinationAddress(network);
        }

        public async Task UpdateBalanceAsync()
        {
            var result = await Insight.GetUnspentCoinsAsync(Address);
            var require = (ulong) Currency.TransactionConfirms;
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

        public bool TryBroadcast(Transaction tx, out string error)
        {
            error = String.Empty;

            try
            {
                Insight.BroadcastAsync(tx).Wait();

                return true;
            }
            catch (AggregateException e)
            {
                if (e.InnerExceptions.Count != 1 || !(e.InnerExceptions[0] is WebException))
                {
                    throw;
                }

                var response = ((WebException) e.InnerExceptions[0]).Response;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    error = reader.ReadToEnd();
                }
            }

            return false;
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
            return Network.NetworkType == NetworkType.Mainnet ? Currency.BIP44CoinType : 0x00000001;
        }
    }
}
