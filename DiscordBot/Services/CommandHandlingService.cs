using CCWallet.DiscordBot.Utilities.Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CCWallet.DiscordBot.Services
{
    public class CommandHandlingService
    {
        public bool Active => (Cancellation != null && !Cancellation.IsCancellationRequested);

        private DiscordSocketClient Discord { get; }
        private IServiceProvider ServiceProvider { get; }
        private CancellationTokenSource Cancellation { get; set; }
        private Dictionary<string, Task> Jobs { get; } = new Dictionary<string, Task>();
        private Dictionary<string, CommandService> Services { get; } = new Dictionary<string, CommandService>();
        private Dictionary<string, BlockingCollection<CommandInfo>> Queues { get; } = new Dictionary<string, BlockingCollection<CommandInfo>>();
        private Dictionary<ulong, ulong> RecentUsers { get; } = new Dictionary<ulong, ulong>();

        internal struct CommandInfo
        {
            public string Command => SearchResult.Commands[0].Command.Name;
            public string Module => SearchResult.Commands[0].Command.Module.Name;
            public string Input;
            public string Prefix;
            public SearchResult SearchResult;
            public CommandService Service;
            public ICommandContext Context;
        }

        public CommandHandlingService(DiscordSocketClient discord, IServiceProvider services)
        {
            Discord = discord;
            ServiceProvider = services;

            Discord.Ready += OnReady;
            Discord.MessageReceived += OnMessageReceived;
        }

        public CommandService AddCommandService(string prefix)
        {
            if (!Services.TryGetValue(prefix, out _))
            {
                Services.Add(prefix, new CommandService());
                Queues.Add(prefix, new BlockingCollection<CommandInfo>());
            }

            return Services[prefix];
        }

        private void Start()
        {
            if (Active)
            {
                Cancellation.Cancel();
                Task.WaitAll(Jobs.Values.ToArray());
            }

            Jobs.Clear();
            Cancellation = new CancellationTokenSource();

            foreach (var prefix in Services.Keys)
            {
                var task = new Task(ProcessCommand, prefix);

                Jobs.Add(prefix, task);
                task.Start();
            }
        }

        private bool TryEnqueue(CommandInfo info)
        {
            return Queues[info.Prefix].TryAdd(info, Timeout.Infinite, Cancellation.Token);
        }

        private bool TryDequeue(string prefix, out CommandInfo info)
        {
            return Queues[prefix].TryTake(out info, Timeout.Infinite, Cancellation.Token);
        }

        private CommandInfo? ParseCommand(SocketUserMessage message)
        {
            var pos = int.MinValue;
            (var prefix, var service) = Services.FirstOrDefault(kv => message?.HasStringPrefix(kv.Key, ref pos) ?? false);

            if (pos >= 0)
            {
                var input = Regex.Replace(message.Content.Substring(pos).Trim(), @"\p{Zs}", " ");
                var context = new CommandContext(Discord, message);

                return new CommandInfo()
                {
                    Input = input,
                    Prefix = prefix,
                    Service = service,
                    Context = context,
                    SearchResult = service.Search(context, input),
                };
            }

            return null;
        }

        private async void ProcessCommand(object obj)
        {
            var prefix = obj as string;

            try
            {
                while (!Cancellation.Token.IsCancellationRequested)
                {
                    if (TryDequeue(prefix, out var command))
                    {
                        var result = await command.Service.ExecuteAsync(command.Context, command.Input, ServiceProvider);

                        if (result.Error == CommandError.Exception)
                        {
                            await command.Context.Message.AddReactionAsync(BotReaction.Error);
                        }
                        else if (!result.IsSuccess)
                        {
                            await command.Context.Message.AddReactionAsync(BotReaction.Unknown);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task OnReady()
        {
            if (!Active)
            {
                await Task.Run(() => Start());
            }
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            var result = ParseCommand(arg as SocketUserMessage);

            if (result.HasValue)
            {
                if (result.Value.SearchResult.IsSuccess)
                {
                    Boolean isLimited = false;
                    List<ulong> toRemove = new List<ulong>();
                    ulong removeThreshold = (arg.Id >> 22) - 60000;

                    foreach (KeyValuePair<ulong, ulong> entry in RecentUsers)
                    {
                        if ((entry.Key >> 22) < removeThreshold)
                        {
                            toRemove.Add(entry.Key);
                        }
                        else if (entry.Value == arg.Author.Id)
                        {
                            isLimited = true;
                        }
                    }
                    foreach(ulong key in toRemove)
                    {
                        RecentUsers.Remove(key);
                    }

                    if (isLimited)
                    {
                        await result.Value.Context.Message.AddReactionAsync(BotReaction.Denied);
                        await result.Value.Context.Message.AddReactionAsync(BotReaction.RateLimited);
                    }
                    else
                    {
                        RecentUsers.Add(arg.Id, arg.Author.Id);
                        await result.Value.Context.Message.AddReactionAsync(BotReaction.InProgress);

                        if (!TryEnqueue(result.Value))
                        {
                            await result.Value.Context.Message.RemoveReactionAsync(BotReaction.InProgress, Discord.CurrentUser);
                            await result.Value.Context.Message.AddReactionAsync(BotReaction.Error);
                        }
                    }
                }
                else
                {
                    await result.Value.Context.Message.AddReactionAsync(BotReaction.Unknown);
                }
            }
        }
    }
}
