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
    public class UserWallet : ICoinSelector
    {
        public IUser User { get; }
        public Network Network { get; }
        public ICurrency Currency { get; }
        public InsightClient Insight { get; }
        public BitcoinAddress Address { get; }
        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;
        public string TotalBalance => Currency.FormatMoney(PendingMoney + ConfirmedMoney, Culture);
        public string PendingBalance => Currency.FormatMoney(PendingMoney, Culture);
        public string ConfirmedBalance => Currency.FormatMoney(ConfirmedMoney, Culture);
        public string UnconfirmedBalance => Currency.FormatMoney(UnconfirmedMoney, Culture);

        private ExtKey ExtKey { get; }
        private List<UnspentOutput.UnspentCoin> UnspentCoins { get; } = new List<UnspentOutput.UnspentCoin>();
        private Money PendingMoney { get; set; } = Money.Zero;
        private Money ConfirmedMoney { get; set; } = Money.Zero;
        private Money UnconfirmedMoney { get; set; } = Money.Zero;

        private static Dictionary<(ulong, Network), HashSet<OutPoint>> UnconfirmedOutPoints { get; } = new Dictionary<(ulong, Network), HashSet<OutPoint>>();

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
            var pending = new HashSet<UnspentOutput.UnspentCoin>();
            var confirmed = new HashSet<UnspentOutput.UnspentCoin>();
            var unconfirmed = new HashSet<UnspentOutput.UnspentCoin>();

            SetConfirmedOutPoints(result.Select(c => c.Outpoint));
            foreach (var coin in result)
            {
                if (HasUnconfirmedOutPoint(coin) || coin.Confirms == 0)
                {
                    unconfirmed.Add(coin);
                }
                else if (coin.Confirms < Currency.TransactionConfirms)
                {
                    pending.Add(coin);
                }
                else if (coin.Confirms >= Currency.TransactionConfirms)
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

        public Transaction BuildTransaction(IDestination destination, decimal amount)
        {
            var builder = Currency.GeTransactionBuilder();

            var tx = builder
                .SetChange(Address)
                .SetCoinSelector(this)
                .SetConsensusFactory(Network)
                .AddKeys(GetExtKey().PrivateKey)
                .AddCoins(UnspentCoins)
                .Send(destination, ConvertMoney(amount))
                .SendFees(Currency.CalculateFee(builder, UnspentCoins))
                .BuildTransaction(true);

            var result = Currency.VerifyTransaction(tx);
            switch (result)
            {
                case TransactionCheckResult.Success: return tx;
                case TransactionCheckResult.OutputTooLarge: throw new ArgumentException("Output is too large.");
                case TransactionCheckResult.OutputTotalTooLarge: throw new ArgumentException("Total output is too large.");
                default: throw new ArgumentException($"TransactionCheck Error: {result}");
            }
        }

        public bool TryBroadcast(Transaction tx, out string error)
        {
            try
            {
                Insight.BroadcastAsync(tx).Wait();
                SetUnconfirmedOutPoints(tx.Inputs.Select(i => i.PrevOut));

                error = String.Empty;
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

        public Money GetFee(Transaction tx)
        {
            return tx.GetFee(UnspentCoins.ToArray());
        }

        public string FormatAmount(Money amount)
        {
            return Currency.FormatMoney(amount, Culture);
        }

        public string FormatAmount(decimal amount)
        {
            return FormatAmount(Money.FromUnit(amount, MoneyUnit.BTC));
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

        private Money ConvertMoney(decimal amount, bool check = true)
        {
            if (check && amount % (1m / Currency.BaseAmountUnit) != 0)
            {
                throw new ArgumentOutOfRangeException(null, "Too many decimal places.");
            }

            if (check && amount < Currency.MinAmount)
            {
                throw new ArgumentOutOfRangeException(null, "Under the minimum amount.");
            }

            if (check && amount > Currency.MaxAmount)
            {
                throw new ArgumentOutOfRangeException(null, "Exceed the maximum amount.");
            }

            return Money.FromUnit(amount, MoneyUnit.BTC);
        }

        private bool HasUnconfirmedOutPoint(ICoin coin)
        {
            return UnconfirmedOutPoints.ContainsKey((User.Id, Network))
                ? UnconfirmedOutPoints[(User.Id, Network)].Contains(coin.Outpoint)
                : false;
        }

        private void SetUnconfirmedOutPoints(IEnumerable<OutPoint> outPoints)
        {
            if (!UnconfirmedOutPoints.ContainsKey((User.Id, Network)))
            {
                UnconfirmedOutPoints[(User.Id, Network)] = new HashSet<OutPoint>();
            }

            UnconfirmedOutPoints[(User.Id, Network)].UnionWith(outPoints.Where(o => !o.IsNull));
        }

        private void SetConfirmedOutPoints(IEnumerable<OutPoint> outPoints)
        {
            if (UnconfirmedOutPoints.ContainsKey((User.Id, Network)))
            {
                UnconfirmedOutPoints[(User.Id, Network)] = UnconfirmedOutPoints[(User.Id, Network)].Intersect(outPoints.Where(o => !o.IsNull)).ToHashSet();

                if (UnconfirmedOutPoints[(User.Id, Network)].Count == 0)
                {
                    UnconfirmedOutPoints.Remove((User.Id, Network));
                }
            }
        }

        IEnumerable<ICoin> ICoinSelector.Select(IEnumerable<ICoin> coins, IMoney target)
        {
            var result = new HashSet<ICoin>();
            var zero = target.Sub(target);

            if (target.CompareTo(zero) > 0 && coins.Count() > 0)
            {
                var total = zero;
                var lower = coins.Where(c => c.Amount.CompareTo(target) <= 0).OrderBy(c => c.Amount);
                var count = lower.Count();

                if (count > 0)
                {
                    for (var i = 0; i < count / 6; i++)
                    {
                        foreach (var c in lower.Skip(i * 5).Take(5))
                        {
                            total = total.Add(c.Amount);
                            result.Add(c);
                        }

                        var tmp = lower.SkipLast(i).Last();
                        total = total.Add(tmp.Amount);
                        result.Add(tmp);

                        if (total.CompareTo(target) >= 0)
                        {
                            return result;
                        }
                    }

                    foreach (var c in lower.Skip(count / 6 * 5).Take(count % 6))
                    {
                        total = total.Add(c.Amount);
                        result.Add(c);

                        if (total.CompareTo(target) >= 0)
                        {
                            return result;
                        }
                    }
                }

                result.Add(coins.Where(c => c.Amount.CompareTo(target) > 0).OrderBy(c => c.Amount).First());
            }

            return result;
        }
    }
}
