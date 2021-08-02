using BotControl;
using System;
using System.Configuration;

namespace Entrance
{
    internal class EntryPoint
    {
        public static void Main()
        {
            var token = ConfigurationManager.AppSettings.Get("BotToken");
            var controller = BotController.GetInstance;
            controller.LaunchBot(token);
            Console.Read();
        }
    }
}
