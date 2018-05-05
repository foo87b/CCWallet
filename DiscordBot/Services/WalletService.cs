using CCWallet.DiscordBot.Utilities;
using Discord;
using NBitcoin;
using System;

namespace CCWallet.DiscordBot.Services
{
    public class WalletService
    {
        private ConfigureService Configure { get; }

        public WalletService(ConfigureService configure)
        {
            Configure = configure;
        }

        public UserWallet GetUserWallet(IUser user)
        {
            // BIP32 path: m / service_index' / user_id1' / user_id2' / user_id3' / reserved'
            return new UserWallet(user, Configure.GetExtKey(GetKeyPath(user.Id)));
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
