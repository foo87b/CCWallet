using System;
using System.Runtime.Serialization;

namespace CCWallet.DiscordBot.Utilities.Insight
{
    [DataContract]
    public class UnspentOutput
    {
        [DataMember(Name = "address")]
        public string Address { get; set; }

        [DataMember(Name = "scriptPubKey")]
        public string ScriptPubKey { get; set; }

        [DataMember(Name = "txid")]
        public string TransactionId { get; set; }

        [DataMember(Name = "amount")]
        public decimal Amount { get; set; }

        [DataMember(Name = "ts")]
        public UInt64 Timestamp { get; set; }

        [DataMember(Name = "vout")]
        public UInt64 ValueOut { get; set; }

        [DataMember(Name = "confirmations")]
        public UInt64 Confirmations { get; set; }
    }
}
