using Topshelf;

namespace BotCoin.WebApi
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
                x.RunAsLocalSystem();
                x.SetDescription("Web API service");
                x.SetDisplayName("BotCoin WebApi");
                x.SetServiceName("BotCoinWebApi");
                x.StartManually();
            });
        }
    }
}
