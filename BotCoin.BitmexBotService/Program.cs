using Topshelf;

namespace BotCoin.BitmexBotService
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
                x.DependsOn("BotCoinWebApi");
                x.RunAsLocalSystem();
                x.SetDescription("Bitmex bot service");
                x.SetDisplayName("BotCoin BitmexBot");
                x.SetServiceName("BotCoinBitmexBot");
                x.StartManually();
            });
        }
    }
}
