using NGettext;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Translations
{
    public class ja : Catalog
    {
        public ja() : base(new CultureInfo("ja"))
        {
            Translations = new Dictionary<string, string[]>
            {
                // Embed Title
                {"Tip", new []{"チップ"} },
                {"Balance", new []{"残高"} },
                {"Withdraw", new []{"送付"} },
                {"Deposit Address", new []{"預入先"} },

                // Embed Field
                {"To", new[]{"送付先"} },
                {"Fee", new[]{"手数料"} },
                {"From", new[]{"送付元"} },
                {"Amount", new[]{"送付量"} },
                {"Result", new[]{"結果"} },
                {"Success", new[]{"成功"} },
                {"Failure", new[]{"失敗"} },
                {"Owner", new[]{"所有者"} },
                {"Confirmed", new []{"検証済"} },
                {"Confirming", new []{"検証中"} },
                {"Unconfirmed", new []{"未検証"} },
                {"Transaction", new[]{"トランザクション"} },

                // Using: BotCommand.Balance
                {"Your {0} balance.", new []{"あなたの{0}の残高です。"} },
                {"Only confirmed balances are available.", new []{"検証済の分のみ使用することが出来ます。"} },
                {"There may be some errors in the balance due to network conditions.", new []{ "ネットワーク状況により残高に多少の誤差が出る場合があります。" } },

                // Using: BotCommand.Deposit
                {"Your {0} deposit address.", new []{"あなたの{0}の預入先です。"} },
                {"Your deposit address is {0}", new []{ "あなたの預入用のアドレスは {0} です。" } },

                // Using: BotCommand.Withdraw
                // Using: BotCommand.Tip
                {"Sent {0}.", new []{ "{0}を送付しました。" } },
                {"Failed to send {0}.", new []{ "{0}の送付に失敗しました。" } },
                {"It will take some time until approved by the network, please check with the Blockchain Explorer.", new []{ "ネットワークに承認されるまで少し時間を要します。ブロックチェーンエクスプローラー等も併せてご確認ください。" } },

                // Transaction Errors
                {"Too many decimal places.", new []{ "小数点以下が細かすぎます。" } },
                {"Under the minimum amount.", new []{ "送付可能な最低量を下回っています。" } },
                {"Exceed the maximum amount.", new []{ "送付可能な最大量を上回っています。" } },
                {"It seems to be invalid address.", new []{ "指定したアドレスが間違っているようです。" } },
                {"It seems to be invalid amount. {0}", new []{ "指定した送付量に問題があるようです。 {0}" } },
                {"It seems to be insufficient funds.", new []{ "ウォレットの残高が不足しているようです。" } },
                {"Transaction could not be generated due to an error. {0}", new []{ "トランザクションの生成に失敗しました。 {0}" } },
                {"Transaction could not be broadcast due to an error. {0}", new []{ "トランザクションの送信に失敗しました。 {0}" } },

            };
        }
    }
}
