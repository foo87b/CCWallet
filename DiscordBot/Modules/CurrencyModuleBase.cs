using CCWallet.DiscordBot.Services;
using CCWallet.DiscordBot.Utilities;
using CCWallet.DiscordBot.Utilities.Discord;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Modules
{
    public abstract class CurrencyModuleBase : ModuleBase
    {
        protected UserWallet Wallet { get; private set; }
        protected abstract Network Network { get; }

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);
            
            Wallet = Provider.GetService<WalletService>().GetUserWallet(Network, Context.User);
            Wallet.CultureInfo = Catalog.CultureInfo;
        }

        [Command(BotCommand.Help)]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions | ChannelPermission.EmbedLinks)]
        public virtual async Task CommandHelpAsync(string command = null)
        {
            throw new NotImplementedException();
        }

        [Command(BotCommand.Balance)]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions | ChannelPermission.EmbedLinks)]
        public virtual async Task CommandBalanceAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            await Wallet.UpdateBalanceAsync();

            await ReplySuccessAsync(_("Your {0} balance.", Wallet.Currency.Name), CreateEmbed(new EmbedBuilder()
            {
                Color = Color.DarkPurple,
                Title = _("Balance"),
                Description = String.Join("\n", new[]
                {
                    _("Only confirmed balances are available."),
                    _("There may be some errors in the balance due to network conditions."),
                }),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder().WithName(_("Owner")).WithValue(GetName(Context.User)),
                    new EmbedFieldBuilder().WithName(_("Balance")).WithValue(Wallet.TotalBalance),
                    new EmbedFieldBuilder().WithIsInline(true).WithName(_("Confirmed")).WithValue(Wallet.ConfirmedBalance),
                    new EmbedFieldBuilder().WithIsInline(true).WithName(_("Confirming")).WithValue(Wallet.PendingBalance),
                    new EmbedFieldBuilder().WithIsInline(true).WithName(_("Unconfirmed")).WithValue(Wallet.UnconfirmedBalance),
                },
            }));
        }

        [Command(BotCommand.Deposit)]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions | ChannelPermission.EmbedLinks)]
        public virtual async Task CommandDepositAsync()
        {
            await Context.Channel.TriggerTypingAsync();
            await ReplySuccessAsync(String.Join("\n", new[]
            {
                _("Your {0} address.", Wallet.Currency.Name),
                Wallet.Address.ToString(),
            }), CreateEmbed(new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Title = _("Deposit Address"),
                Description = String.Join("\n", new[]
                {
                    _("Your deposit address is {0}", Wallet.Address),
                }),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder().WithName(_("Owner")).WithValue(GetName(Context.User)),
                    new EmbedFieldBuilder().WithName(_("Deposit Address")).WithValue($"```{Wallet.Address}```"),
                },
            }));
        }

        [Command(BotCommand.Withdraw)]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions | ChannelPermission.EmbedLinks)]
        public virtual async Task CommandWithdrawAsync(string address, decimal amount)
        {
            await Context.Channel.TriggerTypingAsync();
            await Wallet.UpdateBalanceAsync();
            TryTransfer(address, amount, out var tx, out var error);

            await ReplyTransferAsync(new EmbedBuilder()
            {
                Title = _("Withdraw"),
            }, tx, address, amount, error);
        }

        [Command(BotCommand.Tip)]
        [RequireContext(ContextType.Guild | ContextType.Group)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions | ChannelPermission.EmbedLinks)]
        public virtual async Task CommandTipAsync(IUser user, decimal amount)
        {
            await Context.Channel.TriggerTypingAsync();
            await Wallet.UpdateBalanceAsync();
            TryTransfer(GetAddress(user), amount, out var tx, out var error);

            await ReplyTransferAsync(new EmbedBuilder()
            {
                Title = _("Tip"),
            }, tx, GetName(user), amount, error);
        }
        
        protected virtual async Task ReplyTransferAsync(EmbedBuilder builder, Transaction tx, string destination, decimal amount, string error)
        {
            var result = error == String.Empty;

            builder.AddField(_("Result"), result ? _("Success") : _("Failure"));
            builder.AddField(_("From"), GetName(Context.User));
            builder.AddField(_("To"), destination);
            builder.AddField(_("Amount"), Wallet.FormatAmount(amount), true);

            if (tx != null)
            {
                builder.AddField(_("Fee"), Wallet.FormatMoney(Wallet.GetFee(tx)), true);
                builder.AddField(_("Transaction"), tx.GetHash());
            }

            if (result)
            {
                builder.Color = Color.DarkerGrey;
                builder.Description = String.Join("\n", new[]
                {
                    _("Sent {0}.", Wallet.Currency.Name),
                    _("It will take some time until approved by the network, please check with the Blockchain Explorer."),
                });

                await ReplySuccessAsync(_("Sent {0}.", Wallet.Currency.Name), CreateEmbed(builder));
            }
            else
            {
                builder.Color = Color.Red;
                builder.Description = String.Join("\n", new[]
                {
                    _("Failed to send {0}.", Wallet.Currency.Name),
                    error,
                });

                await ReplyFailureAsync(_("Failed to send {0}.", Wallet.Currency.Name), CreateEmbed(builder));
            }
        }

        protected virtual bool TryTransfer(string address, decimal amount, out Transaction tx, out string error)
        {
            try
            {
                return TryTransfer(BitcoinAddress.Create(address, Network), amount, out tx, out error);
            }
            catch (FormatException)
            {
                error = _("It seems to be invalid address.");
            }

            tx = null;

            return false;
        }

        protected virtual bool TryTransfer(IDestination destination, decimal amount, out Transaction tx, out string error)
        {
            try
            {
                tx = Wallet.BuildTransaction(destination, amount);

                if (Wallet.TryBroadcast(tx, out var result))
                {
                    error = String.Empty;
                    return true;
                }

                error = _("Transaction could not be broadcast due to an error. {0}", _(result));
            }
            catch (NotEnoughFundsException)
            {
                error = _("It seems to be insufficient funds.");
            }
            catch (ArgumentOutOfRangeException e)
            {
                error = _("It seems to be invalid amount. {0}", _(e.Message));
            }
            catch (ArgumentException e)
            {
                error = _("Transaction could not be generated due to an error. {0}", _(e.Message));
            }

            tx = null;

            return false;
        }

        protected virtual Embed CreateEmbed(EmbedBuilder embed = null)
        {
            return (embed ?? new EmbedBuilder())
                .WithAuthor(new EmbedAuthorBuilder()
                {
                    Name = _("{0} Wallet", Wallet.Currency.Name),
                    IconUrl = Wallet.Currency.IconUrl,
                })
                .WithFooter(new EmbedFooterBuilder()
                {
                    Text = _("CCWallet ({0} Module)", Wallet.Currency.Name),
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                })
                .WithThumbnailUrl(GetAvatarUrl(Context.User))
                .WithCurrentTimestamp()
                .Build();
        }

        protected virtual string GetName(IUser user)
        {
            var full = $"{user.Username}#{user.Discriminator}";
            var nick = (user as IGuildUser)?.Nickname;

            return nick != null ? $"{nick} ({full})" : full;
        }

        protected virtual string GetAvatarUrl(IUser user)
        {
            return String.IsNullOrEmpty(user.AvatarId)
                ? $"{DiscordConfig.CDNUrl}embed/avatars/{user.DiscriminatorValue % 5}.png"
                : user.GetAvatarUrl();
        }

        protected virtual BitcoinAddress GetAddress(IUser user)
        {
            return Provider.GetService<WalletService>().GetUserWallet(Network, user).Address;
        }
    }
}
