using CCWallet.DiscordBot.Services;
using CCWallet.DiscordBot.Utilities.Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Modules
{
    [Name("debug")]
    public class DebugModule : ModuleBase
    {
        public WalletService Wallet { get; set; }

        [Command("ping")]
        public async Task CommandPingAsync()
        {
            await Task.WhenAll(new List<Task>()
            {
                Context.Message.AddReactionAsync(BotReaction.Success),
                ReplyAsync($"{Context.User.Mention} pong"),
            });
        }

        [Command("wallet")]
        public async Task CommandWalletAsync(string key)
        {
            var wallet = Wallet.GetUserWallet(key, Context.User);
            await wallet.UpdateBalanceAsync();

            await Task.WhenAll(new List<Task>()
            {
                Context.Message.AddReactionAsync(BotReaction.Success),
                ReplyAsync($"{Context.User.Mention} Address={wallet.Address}, Total={wallet.TotalBalance}, Pending={wallet.PendingBalance}, Unconfirmed={wallet.UnconfirmedBalance}"),
            });
        }
    }
}
