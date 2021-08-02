using BotTranslator;
using Languages;
using ReceivedTranslated;
using System;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotControl
{
    internal class BotController
    {
        private TelegramBotClient Bot = null;
        private static BotController instance = null;
        public static BotController GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BotController();
                }

                return instance;
            }
        }
        private BotController() { }

        public void LaunchBot(string token)
        {
            if (Bot == null)
            {
                Bot = new TelegramBotClient(token);
                ListenMessageAsync();
            }
            else
            {
                throw new InvalidOperationException("The bot is already running");
            }
        }

        private async void ListenMessageAsync()
        {
            await Bot.SetWebhookAsync(string.Empty);
            int offset = 0;
            while (true)
            {
                var updates = await Bot.GetUpdatesAsync(offset);
                foreach (var update in updates)
                {
                    if (update.Type == UpdateType.InlineQuery)
                    {
                        ExecuteQueryAsync(update.InlineQuery);
                    }

                    if (update.Message != null)
                    {
                        var message = update.Message;
                        if (message.Type == MessageType.Text && !string.IsNullOrEmpty(message.ToString()))
                        {
                            ParseMessage(message);
                        }
                    }
                    offset = update.Id + 1;
                }
            }
        }

        private void ParseMessage(Message message)
        {
            var text = message.Text;
            var keyboard = GetKeyboard("Руководство", "Обозначения языков");
            switch (text)
            {
                case "/start":
                    ExecuteMessageAsync(message, keyboard, Strings.answerStart);
                    break;

                case "Руководство":
                case "/manual":
                    ExecuteMessageAsync(message, keyboard, Strings.answerManual);
                    break;

                case "Обозначения языков":
                case "/help":
                    var helpKeyboard = GetKeyboard("Показать пример");
                    ExecuteMessageAsync(message, helpKeyboard, Strings.answerHelp);
                    break;

                case "Показать пример":
                    ExecuteMessageAsync(message, keyboard, Strings.answerExample);
                    break;

                default:
                    var translation = GetTranslations(text);
                    if (translation == null)
                    {
                        ExecuteMessageAsync(message, keyboard, Strings.answerUnknow);
                    }
                    else
                    {
                        ExecuteMessageAsync(message, keyboard, Translator.GetTranslationText(translation));
                    }
                    break;
            }
        }

        private async void ExecuteMessageAsync(Message message, ReplyKeyboardMarkup replyKeyboard, string answer)
        {
            try
            {
            await Bot.SendTextMessageAsync(message.Chat.Id,
                        answer,
                        replyMarkup: replyKeyboard);
            }
            catch(ApiRequestException e)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id,
                        "Перевести не удалось",
                        replyMarkup: replyKeyboard);
                Console.WriteLine(e.Message);
            }
        }

        private async void ExecuteQueryAsync(InlineQuery inlineQuery)
        {
            var translation = GetTranslations(inlineQuery.Query);
            if (translation == null)
            {
                return;
            }
            var translatedText = Translator.GetTranslationText(translation);
            if (translatedText == null) return;
            InlineQueryResultBase[] results = {
                       new InlineQueryResultArticle(
                           id: inlineQuery.Id,
                             title: translatedText,
                              new InputTextMessageContent(translatedText)
                              {
                                  ParseMode = ParseMode.Default
                              })
                       {
                         Description = translation.Text
                       }
                       };
            try
            {
                await Bot.AnswerInlineQueryAsync(inlineQuery.Id, results);
            }
            catch(ApiRequestException e){
                Console.WriteLine(e.Message);
            }
        }

        private static ReplyKeyboardMarkup GetKeyboard(params string[] btnNames)
        {
            var buttons = new List<KeyboardButton>();
            foreach (var e in btnNames)
            {
                buttons.Add(new KeyboardButton(e));
            }
            var keyboard = new ReplyKeyboardMarkup
            {
                Keyboard = new[] { buttons.ToArray() },
                ResizeKeyboard = true
            };
            return keyboard;
        }

        private static Translations GetTranslations(string text)
        {
            if (text.Length > 3 && text[3] == ' ' && Enum.IsDefined(typeof(Language), text.Substring(1, 2)))
            {
                var toLanguage = text.Substring(1, 2);
                text = text[3..];
                Translations result = new() { To = toLanguage, Text = text };
                return result;
            }
            return null;
        }
    }
}

