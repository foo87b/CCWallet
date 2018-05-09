using NBitcoin;
using NBitcoin.DataEncoders;
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

        [DataMember(Name = "confirmations", EmitDefaultValue = false)]
        public UInt64 Confirmations { get; set; }

        public class UnspentCoin : Coin
        {
            public DateTimeOffset Time { get; }
            public int Confirms { get; }

            public UnspentCoin(UnspentOutput output)
            {
                Time = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(output.Timestamp));
                Confirms = Convert.ToInt32(output.Confirmations);
                Outpoint = new OutPoint(uint256.Parse(output.TransactionId), Convert.ToUInt32(output.ValueOut));
                TxOut = new TxOut(Money.FromUnit(output.Amount, MoneyUnit.BTC), new Script(Encoders.Hex.DecodeData(output.ScriptPubKey)));
            }
        }

        public UnspentCoin ToUnspentCoin()
        {
            return new UnspentCoin(this);
        }
    }
}
