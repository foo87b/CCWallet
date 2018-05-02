using System;
using DotNetEnv;
using System.IO;

namespace CCWallet.DiscordBot.Services
{
    public class ConfigureService
    {
        public const string DotEnvFile = ".env";

        internal string DiscordToken => Env.GetString("DISCORD_TOKEN") ?? throw new InvalidOperationException();

        public ConfigureService(string directory = null)
        {
            var path = Path.Combine(directory ?? Directory.GetCurrentDirectory(), DotEnvFile);

            Load(path);
        }

        private void Load(string path)
        {
            if (File.Exists(path))
            {
                Env.Load(path);
            }
        }
    }
}

