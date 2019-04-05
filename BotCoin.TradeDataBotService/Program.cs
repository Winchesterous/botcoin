using Topshelf;

namespace BotCoin.TradeDataBotService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<SelfHost>(s =>
                {
                    s.ConstructUsing(name => new SelfHost());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.DependsOnEventLog();
                x.SetDescription("Collecting data from cryptocurrency exchanges");
                x.SetDisplayName("BotCoin TradeDataBot");
                x.SetServiceName("BotCoinTradeDatabot");
                x.RunAsLocalSystem();
                x.StartManually();
            });
        }
    }
}
