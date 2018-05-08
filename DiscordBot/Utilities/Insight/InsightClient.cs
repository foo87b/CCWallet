using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
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
            
            return (await FetchAsync<IList<UnspentOutput>>(builder.Uri)).ToDictionary(utxo => utxo.ToCoin(), utxo => utxo.Confirmations);
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
