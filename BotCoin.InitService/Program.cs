using Topshelf;

namespace BotCoin.InitService
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
                x.DependsOn(SelfHost.BotcoinTradeDataService);
                x.RunAsLocalSystem();
                x.SetDescription("Service allows periodically started and stopped trade data bot");
                x.SetDisplayName("BotCoin InitService");
                x.SetServiceName("BotCoinInitService");
                x.StartManually();
            });
        }
    }
}
