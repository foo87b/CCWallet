using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;

namespace CCWallet.DiscordBot
{
    class Program
    {
        private static ManualResetEvent WaitHandle { get; } = new ManualResetEvent(false);
        private static IServiceProvider ServiceProvider { get; set; }

        static async Task Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += OnCancelKeyPress;
                ServiceProvider = ConfigureServices().BuildServiceProvider();

                var config = ServiceProvider.GetRequiredService<Services.ConfigureService>();
                var command = ServiceProvider.GetRequiredService<Services.CommandHandlingService>();
                var discord = ServiceProvider.GetRequiredService<DiscordSocketClient>();

                await discord.LoginAsync(TokenType.Bot, config.DiscordToken);
                await discord.StartAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
#if DEBUG
                Console.WriteLine("Press Any Key To Exit...");
                Console.Read();
#endif
                Environment.Exit(1);
            }

            WaitHandle.WaitOne();
        }

        private static IServiceCollection ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<Services.ConfigureService>()
                .AddSingleton<Services.CommandHandlingService>()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
                {
                    DefaultRetryMode = RetryMode.AlwaysRetry,
                }));
        }

        private static async void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            try
            {
                var discord = ServiceProvider.GetService<DiscordSocketClient>();

                if (discord.LoginState == LoginState.LoggedIn ||
                    discord.LoginState == LoginState.LoggingIn)
                {
                    await discord.LogoutAsync();
                }

                if (discord.ConnectionState == ConnectionState.Connected ||
                    discord.ConnectionState == ConnectionState.Connecting)
                {
                    await discord.StopAsync();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            e.Cancel = true;
            WaitHandle.Set();
        }
    }
}
