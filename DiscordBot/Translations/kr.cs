using NGettext;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Translations
{
    public class kr : Catalog
    {
        public kr() : base(new CultureInfo("kr"))
        {
            Translations = new Dictionary<string, string[]>
            {
                // Embed Title
                {"Tip", new []{"팁"} },
                {"Balance", new []{"잔액"} },
                {"Withdraw", new []{"출금"} },
                {"Deposit Address", new []{"입금 주소"} },

                // Embed Field
                {"To", new[]{"To"} },
                {"From", new[]{"From"} },
                {"Amount", new[]{"수량"} },
                {"Result", new[]{"결과"} },
                {"Success", new[]{"성공"} },
                {"Failure", new[]{"실패"} },
                {"Owner", new[]{"소유자"} },
                {"Confirmed", new []{"승인됨"} },
                {"Confirming", new []{"승인 진행중"} },
                {"Unconfirmed", new []{"승인 실패"} },
                {"Transaction", new[]{"트랜잭션"} },

                // Using: BotCommand.Balance
                {"Your {0} balance.", new []{"당신의 잔액은 {0}입니다."} },
                {"Only confirmed balances are available.", new []{"승인이 완료된 잔액만 사용할 수 있습니다."} },
                {"There may be some errors in the balance due to network conditions.", new []{ "네트워크 상황에 따라 잔액에 약간의 에러가 있을 수 있습니다." } },

                // Using: BotCommand.Deposit
                {"Your {0} deposit address.", new []{"당신의 {0}입금 주소입니다."} },
                {"Your deposit address is {0}", new []{ "당신의 입금 주소는 {0}입니다." } },

                // Using: BotCommand.Withdraw
                // Using: BotCommand.Tip
                {"Sent {0}.", new []{ "{0}를 보냈습니다." } },
                {"Failed to send {0}.", new []{ "{0}를 보내는데 실패했습니다." } },
                {"It will take some time until approved by the network, please check with the Blockchain Explorer.", new []{ "네트워크 승인까지 시간이 더 소요됩니다. 블록체인 익스플로러를 확인해 주세요." } },

                // Transaction Errors
                {"Too many decimal places.", new []{ "소수점 이하 숫자가 너무 많습니다." } },
                {"Under the minimum amount.", new []{ "최소 전송 가능 수량보다 적습니다." } },
                {"Exceed the maximum amount.", new []{ "최대 전송 가능 수량보다 많습니다." } },
                {"It seems to be invalid address.", new []{ "송금 주소가 잘못되었습니다." } },
                {"It seems to be invalid amount. {0}", new []{ "송금 수량이 잘못되었습니다. {0}" } },
                {"It seems to be insufficient funds.", new []{ "지갑의 잔액이 부족합니다." } },
                {"Transaction could not be generated due to an error. {0}", new []{ "트랜잭션 생성에 실패했습니다. {0}" } },
                {"Transaction could not be broadcast due to an error. {0}", new []{ "트랜잭션 송신에 실패했습니다. {0}" } },

            };
        }
    }
}
