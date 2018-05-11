using CCWallet.DiscordBot.Services;
using CCWallet.DiscordBot.Utilities.Discord;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using NGettext;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Modules
{
    public class ModuleBase : Discord.Commands.ModuleBase
    {
        public IServiceProvider Provider { get; set; }
        public Catalog Catalog { get; protected set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            Catalog = Provider.GetService<CultureService>().GetCatalog(Context.Channel);
        }

        protected virtual string _(string text)
        {
            return Catalog.GetString(text);
        }

        protected virtual string _(string text, params object[] args)
        {
            return Catalog.GetString(text, args);
        }

        protected virtual void SetLanguage(string lang, bool remaind = true)
        {
            var culture = Provider.GetService<CultureService>();

            if (remaind)
            {
                culture.SetLanguage(Context.Channel, lang);
            }

            Catalog = culture.GetCatalog(lang);
        }

        protected virtual async Task ReplySuccessAsync(string message, Embed embed = null)
        {
            await Task.WhenAll(new List<Task>()
            {
                Context.Message.AddReactionAsync(BotReaction.Success),
                ReplyAsync($"{Context.User.Mention} {message}", false, embed),
            });
        }

        protected virtual async Task ReplyFailureAsync(string message, Embed embed = null)
        {
            await Task.WhenAll(new List<Task>()
            {
                Context.Message.AddReactionAsync(BotReaction.Failure),
                ReplyAsync($"{Context.User.Mention} {message}", false, embed),
            });
        }
    }
}
