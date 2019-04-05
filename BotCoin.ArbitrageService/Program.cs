using Topshelf;

namespace BotCoin.ArbitrageService
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

                x.DependsOnMsSql();
                x.DependsOnEventLog();
                x.SetDescription("BotCoin matching engine for implemented arbitrage strategy.");
                x.SetDisplayName("BotCoin Arbitrage");
                x.SetServiceName(SelfHost.ServiceName);
                x.RunAsLocalSystem();
                x.StartManually();
            });
        }
    }
}
