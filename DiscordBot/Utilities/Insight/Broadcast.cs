using NBitcoin;
using System.Runtime.Serialization;

namespace CCWallet.DiscordBot.Utilities.Insight
{
    [DataContract]
    public class Broadcast
    {
        [DataMember(Name = "rawtx")]
        public string RawTransaction { get; set; }

        public static Broadcast ConvertFrom(Transaction tx)
        {
            return new Broadcast() {RawTransaction = tx.ToHex()};
        }
    }
}
