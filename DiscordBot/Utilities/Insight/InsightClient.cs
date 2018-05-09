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

        public async Task<IEnumerable<UnspentOutput.UnspentCoin>> GetUnspentCoinsAsync(BitcoinAddress address)
        {
            var builder = new UriBuilder(BaseUri);
            builder.Path += $"/addr/{address}/utxo";
            builder.Query = "noCache=1";
            
            return (await FetchAsync<IList<UnspentOutput>>(builder.Uri)).Select(u => u.ToUnspentCoin());
        }

        public async Task BroadcastAsync(Transaction tx)
        {
            var builder = new UriBuilder(BaseUri);
            builder.Path += "/tx/send";

            await PostAsync(builder.Uri, Broadcast.ConvertFrom(tx));
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

        private async Task<WebResponse> PostAsync<T>(Uri uri, T param) where T : class 
        {
            var request = WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/json";

            using (var stream = request.GetRequestStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(stream, param);
            }

            return await request.GetResponseAsync();
        }
    }
}
