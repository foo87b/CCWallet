using Discord;
using NBitcoin;
using System;

namespace CCWallet.DiscordBot.Utilities
{
    public class UserWallet
    {
        public IUser User { get; }

        private ExtKey ExtKey { get; }

        internal UserWallet(IUser user, ExtKey key)
        {
            User = user;
            ExtKey = key;
        }

        public BitcoinAddress GetAddress(Network network)
        {
            return GetExtKey(network).ScriptPubKey.GetDestinationAddress(network);
        }

        private ExtKey GetExtKey(Network network, int account = 0, int change = 0, int index = 0)
        {
            var type = 0;

            // SLIP-0044: Registered coin types for BIP-0044
            // if not implementing BIP-0044 currency, use numbers above 0x70000000
            switch (network.Magic)
            {
                // eXperience Points: mainnet
                case 0xe5e2f8b4:
                    type = 0x70000001;
                    break;

                default:
                    throw new ArgumentException();
            }

            // BIP32 path: m / purpose' / coin_type' / account' / change / address_index
            return ExtKey.Derive(GetKeyPath(type: type, account: account, change: change, index: index));
        }

        private KeyPath GetKeyPath(int purpose = 44, int type = 0, int account = 0, int change = 0, int index = 0)
        {
            // BIP-0044: Multi-Account Hierarchy for Deterministic Wallets
            return new KeyPath(new uint[]
            {
                0x80000000 | Convert.ToUInt32(purpose),
                0x80000000 | Convert.ToUInt32(type),
                0x80000000 | Convert.ToUInt32(account),
                0x7FFFFFFF & Convert.ToUInt32(change),
                0x7FFFFFFF & Convert.ToUInt32(index),
            });
        }
    }
}
