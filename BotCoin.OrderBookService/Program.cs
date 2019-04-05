using Topshelf;

namespace BotCoin.OrderBookService
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
                x.SetDescription("BotCoin order books and prices monitoring service.");
                x.SetDisplayName("BotCoin Order Books");
                x.SetServiceName("BotcoinOrderBooks");
                x.RunAsLocalSystem();
                x.StartManually();
            });
        }
    }
}
