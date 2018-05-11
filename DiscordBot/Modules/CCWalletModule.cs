using CCWallet.DiscordBot.Services;
using CCWallet.DiscordBot.Utilities.Discord;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Modules
{
    [Name("ccwallet")]
    public class CCWalletModule : ModuleBase
    {
        [Command("ping")]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions)]
        public async Task CommandPingAsync()
        {
            await ReplySuccessAsync(_("pong"));
        }

        [Command("lang")]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
        [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.AddReactions)]
        public async Task CommandLangAsync(string lang = null)
        {
            if (Context.User is IGuildUser && !((IGuildUser) Context.User).GuildPermissions.Administrator)
            {
                await Context.Message.AddReactionAsync(BotReaction.Denied);

                return;
            }

            if (String.IsNullOrEmpty(lang))
            {
                await ReplySuccessAsync(_("Current language is {0}.", Catalog.CultureInfo.Name));

                return;
            }

            try
            {
                var culture = Provider.GetService<CultureService>();

                SetLanguage(lang);

                await ReplySuccessAsync(String.Join("\n", new[]
                {
                    _("Changed language to {0}.", Catalog.CultureInfo.Name),
                    culture.HasTranslation(Catalog.CultureInfo.Name)
                        ? _("It displays in the specified language.")
                        : _("Since it has not been translated yet, it is displayed in the default language."),
                }));
            }
            catch (CultureNotFoundException)
            {
                await ReplyFailureAsync(_("Unknown language. ({0})", lang));
            }
        }
    }
}
