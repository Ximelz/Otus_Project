using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Otus_Project_Manage
{
    /// <summary>
    /// Сценарий удаления пользователя.
    /// </summary>
    public class DeleteUserScenario : IScenario
    {
        public DeleteUserScenario(IUserService userService)
        {
            this.userService = userService;
        }
        private IUserService userService;
        public ScenarioTypes ScenarioType => ScenarioTypes.DeleteUser;

        public async Task<ScenarioStatus> HandleScanarioAsync(ITelegramMessageService telegramMessageService, UserScenarioData userScenario)
        {
            telegramMessageService.ct.ThrowIfCancellationRequested();

            if (telegramMessageService.update.Type != UpdateType.CallbackQuery)
            {
                await telegramMessageService.SendMessage("Для работы сценария необходимо получить информацию из кнопки, а не из ввода сообщения!");
                return ScenarioStatus.InProcess;
            }

            CallbackQueryData callbackQueryData = new CallbackQueryData(telegramMessageService.update);
            Guid userId;

            switch (userScenario.currentStep)
            {
                case "Start":
                    if (!Guid.TryParse(callbackQueryData.Argument, out userId))
                    {
                        await telegramMessageService.SendMessage("Нажата неверная кнопка!");
                    }

                    if (!userScenario.Data.ContainsKey("UserId"))
                        userScenario.Data.Add("UserId", callbackQueryData.Argument);
                    else
                        userScenario.Data["UserId"] = callbackQueryData.Argument;

                    InlineKeyboardMarkup keyboard = new InlineKeyboardMarkup();

                    keyboard.AddNewRow(new InlineKeyboardButton[] {
                                           InlineKeyboardButton.WithCallbackData("Да", $"AcceptDeleteUser|{callbackQueryData.Argument}"),
                                           InlineKeyboardButton.WithCallbackData("Нет", $"CancelDeleteUser|{callbackQueryData.Argument}")});

                    var user = await userService.GetUserByUserId(userId, telegramMessageService.ct);

                    await telegramMessageService.SendMessageWithKeyboard($"Вы хотите удалить пользователя {user.userName}?", keyboard);

                    userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    userScenario.currentStep = "DeleteUser";

                    return userScenario.scenarioStatus;
                case "DeleteUser":
                    if (callbackQueryData.Command == "AcceptDeleteUser")
                    {
                        Guid.TryParse(userScenario.Data["UserId"].ToString(), out userId);
                        await userService.DeleteUser(userId, telegramMessageService.ct);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;

                        await telegramMessageService.SendMessageByKeyboardType("Пользователь удален!", KeyboardTypes.Admin);
                    }
                    else if (callbackQueryData.Command == "CancelDeleteUser")
                    {
                        await telegramMessageService.SendMessageByKeyboardType("Удаление отменено!", KeyboardTypes.Admin);
                        userScenario.scenarioStatus = ScenarioStatus.Completed;
                    }
                    else
                    {
                        await telegramMessageService.SendMessage("Неверная команда!");
                        userScenario.scenarioStatus = ScenarioStatus.InProcess;
                    }
                    return userScenario.scenarioStatus;
                default:
                    await telegramMessageService.SendMessage("Неверный этап для уделаения пользователя.");
                    userScenario.scenarioStatus = ScenarioStatus.InProcess;

                    return userScenario.scenarioStatus;
            }
        }
    }
}
