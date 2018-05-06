using DotNetEnv;
using NBitcoin;
using System;
using System.IO;
using System.Net;
using System.Security;

namespace CCWallet.DiscordBot.Services
{
    public class ConfigureService
    {
        public const string DotEnvFile = ".env";

        internal string DiscordToken => Env.GetString("DISCORD_TOKEN") ?? throw new InvalidOperationException();

        private ExtKey WalletMasterKey { get; }
        private string WalletPrivateKey => Env.GetString("WALLET_PRIVATE_KEY") ?? throw new InvalidOperationException();
        private string WalletExtendedPublicKey => Env.GetString("WALLET_EXTENDED_PUBLIC_KEY");

        public ConfigureService(string directory = null)
        {
            Load(directory);

            // BIP32 path: m / feature_use' / *
            WalletMasterKey = GetMasterKey().Derive(0, true);
        }

        public string GetString(string key, string fallback = null)
        {
            return Env.GetString(key, fallback);
        }

        private void Load(string directory)
        {
            var path = Path.Combine(directory ?? Directory.GetCurrentDirectory(), DotEnvFile);

            if (File.Exists(path))
            {
                Env.Load(path);
            }
        }
        
        internal ExtKey GetExtKey(KeyPath derivation)
        {
            // BIP32 path: m / *
            return WalletMasterKey.Derive(derivation);
        }

        private ExtKey GetMasterKey()
        {
            if (String.IsNullOrEmpty(WalletExtendedPublicKey))
            {
                return ExtKey.Parse(WalletPrivateKey);
            }

            var xpub = ExtPubKey.Parse(WalletExtendedPublicKey);
            var password = default(SecureString);

            try
            {
                var encrypted = BitcoinEncryptedSecret.Create(WalletPrivateKey);
                password = GetPassword();

                return new ExtKey(xpub, encrypted.GetKey(new NetworkCredential(String.Empty, password).Password));
            }
            catch (FormatException)
            {
                return new ExtKey(xpub, Key.Parse(WalletPrivateKey));
            }
            catch (SecurityException)
            {
                Console.WriteLine("Invalid password.");
#if DEBUG
                Console.WriteLine("Press Any Key To Exit...");
                Console.Read();
#endif
                Environment.Exit(1);
                return null;
            }
            finally
            {
                password?.Dispose();
            }
        }

        private SecureString GetPassword()
        {
            var password = new SecureString();
            Console.Write("Password: ");

            while (true)
            {
                var input = Console.ReadKey(true);

                if (input.Key == ConsoleKey.Enter)
                {
                    password.MakeReadOnly();
                    Console.WriteLine();

                    return password;
                }
                else if (input.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.RemoveAt(password.Length - 1);
                }
                else if (input.KeyChar != 0)
                {
                    password.AppendChar(input.KeyChar);
                }
            }
        }
    }
}
