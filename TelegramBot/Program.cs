using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;

namespace TelegramBot
{

    class Program
    {
        static void Main(string[] args)
        {
            ContestBot bot = new ContestBot(TOKEN);//TestBot
            bot.Start();
        }
    }
}
