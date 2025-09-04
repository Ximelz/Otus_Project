using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    public class KeyboardCommands
    {
        public static List<BotCommand> GetUserCommands(ProjectUser user)
        {
            List<BotCommand> currentCommands = new List<BotCommand>();

            if (user.role == UserRole.None)
                currentCommands.Add(new BotCommand { Command = "start", Description = "Авторизация и запуск бота" });

            if (user.role == UserRole.TeamLead)
                currentCommands.Add(new BotCommand { Command = "showteamtasks", Description = "Просмотр задач команды." });

            if (user.role != UserRole.None)
                currentCommands.Add(new BotCommand { Command = "showmytasks", Description = "Просмотр моих задач." });

            if (user.project != null)
                currentCommands.Add(new BotCommand { Command = "showprojects", Description = "Просмотр моих задач." });

            currentCommands.Add(new BotCommand { Command = "info", Description = "Информация о боте." });
            currentCommands.Add(new BotCommand { Command = "help", Description = "Информация о работе с ботом." });

            if (user.isAdmin)
                currentCommands.Add(new BotCommand { Command = "adminconsole", Description = "Консоль администратора." });

            return currentCommands;
        }

        public static ReplyKeyboardMarkup GetKeyboardMarkup(KeyboardTypes keyboardType)
        {
            if (keyboardType == KeyboardTypes.None)
                return new ReplyKeyboardMarkup(new[] { new KeyboardButton("/start") });

            if (keyboardType == KeyboardTypes.ProjectLead)
                return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("/addteamtask"),
                    new KeyboardButton("/showteamtasks"),
                    new KeyboardButton("/showmytasks")
                });

            if (keyboardType == KeyboardTypes.TeamLead)
                return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("/showmytasks")
                });

            if (keyboardType == KeyboardTypes.Admin)
                return new ReplyKeyboardMarkup(
                new[]
                {
                    new KeyboardButton("/registerUser"),
                    new KeyboardButton("/showUsers"),
                    new KeyboardButton("/showTeams"),
                    new KeyboardButton("/showAllprojects"),
                    new KeyboardButton("/exitAdminConsole")
                });

            if (keyboardType == KeyboardTypes.CancelScenario)
                return new ReplyKeyboardMarkup(new[] { new KeyboardButton("/cancel") });

            return new ReplyKeyboardMarkup(new[] { new KeyboardButton("/showmytasks") });
        }

        public static KeyboardTypes GetKeyoardTypeByRole(UserRole role)
        {
            switch (role)
            {
                case UserRole.TeamLead:
                    return KeyboardTypes.TeamLead;
                default:
                    return KeyboardTypes.Default;
            }
        }
    }
}