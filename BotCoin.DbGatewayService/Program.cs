using Topshelf;

namespace BotCoin.DbGatewayService
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
                x.SetDescription("BotCoin database gateway for the matching instances.");
                x.SetDisplayName("BotCoin Database Gateway");
                x.SetServiceName("BotcoinDbGateway");
                x.RunAsLocalSystem();
                x.StartManually();
            });
        }
    }
}
