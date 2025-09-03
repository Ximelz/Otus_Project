using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otus_Project_Manage
{
    public class CallbackQueryData
    {
        public CallbackQueryData(Update update)
        {
            string[] inputStr = update.CallbackQuery.Data.Split('|');
            Command = inputStr[0];
            Argument = inputStr[1];
            telegramUserId = update.CallbackQuery.From.Id;
        }
        public readonly string Command;
        public readonly string Argument;
        public readonly long telegramUserId;
    }
}
