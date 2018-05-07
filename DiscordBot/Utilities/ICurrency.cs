using NBitcoin;
using System.Globalization;

namespace CCWallet.DiscordBot.Utilities
{
    public interface ICurrency : INetworkSet
    {
        string Name { get; }
        int BIP44CoinType { get; }
        int TransactionConfirms { get; }

        string FormatBalance(Money money, CultureInfo culture, bool symbol = true);
    }
}
