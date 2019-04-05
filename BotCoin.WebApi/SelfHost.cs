using BotCoin.DataType;
using BotCoin.Logger;
using BotCoin.Service;
using Nancy;
using Nancy.Hosting.Self;
using System;
using System.Configuration;
using System.Net;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace BotCoin.WebApi
{
    public class CustomBootStrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            Nancy.Json.JsonSettings.MaxJsonLength = int.MaxValue;
            Nancy.Json.JsonSettings.MaxRecursions = 100;
            Nancy.Json.JsonSettings.RetainCasing = true;
            base.ApplicationStartup(container, pipelines);
        }
    }

    public class SelfHost
    {
        internal static ServiceEventLogger Log;
        readonly NancyHost _selfHost;

        static SelfHost()
        {
            Log = new ServiceEventLogger(ServiceName.WebApi, new DbRepositoryService());
        }

        public SelfHost()
        {
            _selfHost = new NancyHost(
                new Uri("http://localhost:" + Int32.Parse(ConfigurationManager.AppSettings["ServicePort"])),
                new CustomBootStrapper(),
                new HostConfiguration() { UrlReservations = new UrlReservations() { CreateAutomatically = true } }
                );
        }

        private string FormatMessage(string msg)
        {
            return String.Format("{0} [{1}]", msg, Dns.GetHostName());
        }

        public void Start() 
        {
            try
            {
                _selfHost.Start();
                Log.WriteInfo(FormatMessage("Service started"));
            }
            catch (Exception ex)
            {
                Log.WriteError(ex);
            }
        }

        public void Stop()
        {
            try
            {
                _selfHost.Stop();
                Log.WriteInfo(FormatMessage("Service stopped"));
            }
            catch (Exception ex)
            {
                Log.WriteError(ex);
            }
        }
    }
}
