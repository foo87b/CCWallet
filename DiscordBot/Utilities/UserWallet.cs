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
        public CultureInfo CultureInfo { get; set; } = CultureInfo.CurrentCulture;
        public BitcoinAddress Address => ScriptPubKey.GetDestinationAddress(Network);
        public string TotalBalance => FormatMoney(PendingMoney + ConfirmedMoney);
        public string PendingBalance => FormatMoney(PendingMoney);
        public string ConfirmedBalance => FormatMoney(ConfirmedMoney);
        public string UnconfirmedBalance => FormatMoney(UnconfirmedMoney);

        private ExtKey ExtKey { get; }
        private Script ScriptPubKey { get; }
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
            ScriptPubKey = GetExtKey().ScriptPubKey;
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

        public Transaction BuildTransaction(Dictionary<IDestination, decimal> outputs)
        {
            var builder = Currency.GeTransactionBuilder();
            builder.SetChange(Address)
                .SetConsensusFactory(Network)
                .AddKeys(GetExtKey().PrivateKey);

            var totalAmount = decimal.Zero;
            foreach (var output in outputs)
            {
                totalAmount += output.Value;
                builder.Send(output.Key, ConvertMoney(output.Value));
            }

            var coins = UnspentCoinSelector(ConvertMoney(totalAmount));
            var tx = builder
                .AddCoins(coins)
                .SendFees(Currency.CalculateFee(builder, coins))
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
                error = String.Empty;
                Insight.BroadcastAsync(tx).Wait();
                SetUnconfirmedOutPoints(tx.Inputs.Select(i => i.PrevOut));

                return true;
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1 && e.InnerExceptions[0] is WebException)
            {
                var response = ((WebException) e.InnerExceptions[0]).Response;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    error = reader.ReadToEnd();
                }

                return false;
            }
        }

        public Money GetFee(Transaction tx)
        {
            return tx.GetFee(UnspentCoins.ToArray());
        }

        public string FormatMoney(Money money)
        {
            return Currency.FormatMoney(money, CultureInfo);
        }

        public string FormatAmount(decimal amount)
        {
            return Currency.FormatAmount(amount, CultureInfo);
        }

        public void ValidateAmount(decimal amount, bool check)
        {
            // force overflow check.
            if (amount > long.MaxValue / (decimal)Money.COIN)
            {
                throw new ArgumentOutOfRangeException(null, "Exceed the maximum amount.");
            }

            if (check && (amount > Currency.MaxAmount))
            {
                throw new ArgumentOutOfRangeException(null, "Exceed the maximum amount.");
            }

            if (check && amount < Currency.MinAmount)
            {
                throw new ArgumentOutOfRangeException(null, "Lower than the minimum transferable amount.");
            }

            if (check && amount % (1m / Currency.BaseAmountUnit) != 0)
            {
                throw new ArgumentOutOfRangeException(null, "Too many numbers after decimal point places.");
            }
        }

        private ExtKey GetExtKey(int account = 0, int change = 0, int index = 0)
        {
            // BIP32 path: m / purpose' / coin_type' / account' / change / address_index
            return ExtKey.Derive(GetKeyPath(type: GetCoinType(), account: account, change: change, index: index));
        }

        private KeyPath GetKeyPath(int purpose = 44, int type = 0, int account = 0, int change = 0, int index = 0)
        {
            // BIP-0044: Multi-Account Hierarchy for Deterministic Wallets
            return new KeyPath(new []
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
            ValidateAmount(amount, check);

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

        private IEnumerable<UnspentOutput.UnspentCoin> UnspentCoinSelector(Money target)
        {
            var coins = CoinSelector(UnspentCoins, target).Select(c => c.Outpoint);

            return UnspentCoins.Where(c => coins.Contains(c.Outpoint));
        }

        private IEnumerable<ICoin> CoinSelector(IEnumerable<ICoin> coins, IMoney target)
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

                if (count < coins.Count())
                {
                    result.Add(coins.Where(c => c.Amount.CompareTo(target) > 0).OrderBy(c => c.Amount).First());
                }
            }

            return result;
        }
    }
}
