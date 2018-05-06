using NBitcoin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Utilities.Insight
{
    public class InsightClient
    {
        public Uri BaseUri { get; }

        public InsightClient(string endpoint)
        {
            BaseUri = new Uri(endpoint.TrimEnd('/'), UriKind.Absolute);
        }

        public async Task<Dictionary<Coin, ulong>> GetUnspentCoinsAsync(BitcoinAddress address)
        {
            var builder = new UriBuilder(BaseUri);
            builder.Path += $"/addr/{address}/utxo";
            builder.Query = "noCache=1";

            var coins = new Dictionary<Coin, ulong>();
            var result = await FetchAsync<IList<UnspentOutput>>(builder.Uri);

            foreach (var utxo in result)
            {
                var txid = uint256.Parse(utxo.TransactionId);
                var vout = Convert.ToUInt32(utxo.ValueOut);
                var amount = Money.FromUnit(utxo.Amount, MoneyUnit.BTC);
                var script = new Script(utxo.ScriptPubKey);

                coins.Add(new Coin(txid, vout, amount, script), utxo.Confirmations);
            }

            return coins;
        }

        private async Task<T> FetchAsync<T>(Uri uri) where T : class
        {
            var request = WebRequest.Create(uri);
            var response = await request.GetResponseAsync();

            using (var stream = response.GetResponseStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));

                return serializer.ReadObject(stream) as T;
            }
        }
    }
}
