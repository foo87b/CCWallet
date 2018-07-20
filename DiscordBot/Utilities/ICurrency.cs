using CCWallet.DiscordBot.Utilities.Insight;
using NBitcoin;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Utilities
{
    public interface ICurrency : INetworkSet
    {
        string Name { get; }
        string IconUrl { get; }
        string MessageMagic { get; }
        int BIP44CoinType { get; }
        int TransactionConfirms { get; }
        int BaseAmountUnit { get; }
        decimal MinAmount { get; }
        decimal MaxAmount { get; }
        decimal MinRainAmount { get; }
        int MaxRainUsers { get; }

        string FormatMoney(Money money, CultureInfo culture, bool symbol = true);
        string FormatAmount(decimal amount, CultureInfo culture, bool symbol = true);
        Money CalculateFee(TransactionBuilder builder, IEnumerable<UnspentOutput.UnspentCoin> unspnets);
        TransactionBuilder GeTransactionBuilder();
        TransactionCheckResult VerifyTransaction(Transaction tx);
    }
}
