using NGettext;
using System.Collections.Generic;
using System.Globalization;

namespace CCWallet.DiscordBot.Translations
{
    public class ko : Catalog
    {
        public ko() : base(new CultureInfo("ko"))
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
                {"Fee", new[]{"수수료"} },
                {"From", new[]{"From"} },
                {"Amount", new[]{"수량"} },
                {"Result", new[]{"결과"} },
                {"Success", new[]{"성공"} },
                {"Failed", new[]{"실패"} },
                {"Owner", new[]{"소유자"} },
                {"Confirmed", new []{"승인 완료"} },
                {"Confirming", new []{"승인중"} },
                {"Unconfirmed", new []{"미승인"} },
                {"Transaction", new[]{"트랜잭션"} },

                // Using: BotCommand.Balance
                {"Your {0} balance.", new []{"당신의 {0} 잔액입니다."} },
                {"Only confirmed balances are available.", new []{"승인이 완료된 잔액만 사용할 수 있습니다."} },
                {"Slight balance errors may occur due to network conditions.", new []{ "네트워크 상황에 따라 잠시 잔액에 에러가 있을 수 있습니다." } },

                // Using: BotCommand.Deposit
                {"Your {0} address.", new []{"당신의 {0}입금 주소입니다."} },
                {"Your deposit address is {0}.", new []{ "당신의 입금 주소는 {0}입니다." } },

                // Using: BotCommand.Withdraw
                // Using: BotCommand.Tip
                {"Sent {0}.", new []{ "{0}를 보냈습니다." } },
                {"Failed to send {0}.", new []{ "{0}를 보내는데 실패했습니다." } },
                {"It may take some time to receive an approved message from the network; you can also check the status with the Blockchain Explorer.", new []{ "네트워크 승인까지 시간이 더 소요됩니다. 블록체인 익스플로러에서 진행 상태를 확인해 주세요." } },

                // Transaction Errors
                {"Too many numbers after decimal point places.", new []{ "소수점 이하 숫자가 너무 많습니다." } },
                {"Lower than the minimum transferable amount.", new []{ "최소 전송 가능 수량보다 적습니다." } },
                {"Exceed the maximum amount.", new []{ "최대 전송 가능 수량보다 많습니다." } },
                {"Invalid address.", new []{ "송금 주소가 잘못되었습니다." } },
                {"Invalid amount. {0}", new []{ "송금 수량이 잘못되었습니다. {0}" } },
                {"Insufficient funds.", new []{ "잔액이 부족합니다." } },
                {"Error - Transaction generation failed. {0}", new []{ "에러 - 트랜잭션 생성에 실패했습니다. {0}" } },
                {"Error - Transaction transmission failed. {0}", new []{ "에러 - 트랜잭션 송신에 실패했습니다. {0}" } },

            };
        }
    }
}
