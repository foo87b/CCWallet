using CCWallet.DiscordBot.Utilities;
using CCWallet.DiscordBot.Utilities.Insight;
using Discord;
using NBitcoin;
using System;
using System.Collections.Generic;

namespace CCWallet.DiscordBot.Services
{
    public class WalletService
    {
        private ConfigureService Configure { get; }
        private Dictionary<string, ICurrency> Currencies { get; } = new Dictionary<string, ICurrency>();
        private Dictionary<string, InsightClient> InsightClients { get; } = new Dictionary<string, InsightClient>();

        public WalletService(ConfigureService configure)
        {
            Configure = configure;
        }

        public void AddCurrency(ICurrency currency, NetworkType network = NetworkType.Mainnet)
        {
            // load "CURRENCY_NETWORK_INSIGHT" environment value.
            var endpoint = Configure.GetString($"{currency.CryptoCode}_{network}_INSIGHT".ToUpper()) ??
                           throw new ArgumentNullException();
            
            Currencies[currency.GetNetwork(network).Name] = currency;
            InsightClients[currency.GetNetwork(network).Name] = new InsightClient(endpoint);
        }

        public ICurrency GetCurrency(Network network)
        {
            return Currencies[network.Name];
        }

        public InsightClient GetInsightClient(Network network)
        {
            return InsightClients[network.Name];
        }

        public UserWallet GetUserWallet(Network network, IUser user)
        {
            // BIP32 path: m / service_index' / user_id1' / user_id2' / user_id3' / reserved'
            return new UserWallet(this, network, user, Configure.GetExtKey(GetKeyPath(user.Id)));
        }

        private KeyPath GetKeyPath(ulong id)
        {
            return new KeyPath(new uint[]
            {
                0x80000000 | 0, // service_index: discord is 0
                0x80000000 | Convert.ToUInt32(id & 0xFFFF000000000000 >> 48), // user_id1: 16bit
                0x80000000 | Convert.ToUInt32(id & 0x0000FFFFFF000000 >> 24), // user_id2: 24bit
                0x80000000 | Convert.ToUInt32(id & 0x0000000000FFFFFF >>  0), // user_id3: 24bit
                0x80000000 | 0, // reserved
            });
        }
    }
}
