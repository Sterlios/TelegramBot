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
            //СompetitionBot bot = new СompetitionBot("5669645963:AAEsExKlzrvMQf0vQ5UhmkDDPtDNpoyUgfM");//NaturinoTestBot
            //СompetitionBot bot = new СompetitionBot("5505851101:AAFWvcFWi-fgrNzyTm0geJbdWHxtYuhydeY");//Naturino_New_Bot
            ContestBot bot = new ContestBot("5700546220:AAH4DMxllrI55rR_tm4wC00a6w12Jei2U3o");//TestBot
            //СompetitionBot bot = new СompetitionBot("5700546220:AAH4DMxllrI55rR_tm4wC00a6w12Jei2U3o");//TestBot
            bot.Start();
        }

    }
}