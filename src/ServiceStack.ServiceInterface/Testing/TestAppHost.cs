using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Funq;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.WebHost.Endpoints;

namespace ServiceStack.ServiceInterface.Testing
{
    public class TestAppHost : IAppHost
    {
        private readonly Funq.Container container;

        public TestAppHost()
            : this(new Container(), Assembly.GetExecutingAssembly()) {}

        public TestAppHost(Funq.Container container, params Assembly[] serviceAssemblies)
        {
            this.container = container ?? new Container();
            if (serviceAssemblies.Length == 0)
                serviceAssemblies = new[] { Assembly.GetExecutingAssembly() };
            
            var createInstance = EndpointHostConfig.Instance;

            this.Config = EndpointHost.Config = new EndpointHostConfig(
                GetType().Name,
                new ServiceManager(true, serviceAssemblies));

            this.ContentTypeFilters = new HttpResponseFilter();
            this.RequestFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.ResponseFilters = new List<Action<IHttpRequest, IHttpResponse, object>>();
            this.HtmlProviders = new List<StreamSerializerResolverDelegate>();
            this.CatchAllHandlers = new List<HttpHandlerResolverDelegate>();
        }

        public void RegisterAs<T, TAs>() where T : TAs
        {
            this.container.RegisterAs<T, TAs>();
        }

        public virtual void Release(object instance) { }

        public void Register<T>(T instance)
        {
            container.Register(instance);
        }

        public T TryResolve<T>()
        {
            return container.TryResolve<T>();
        }

        public IContentTypeFilter ContentTypeFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> RequestFilters { get; set; }

        public List<Action<IHttpRequest, IHttpResponse, object>> ResponseFilters { get; set; }

        public List<StreamSerializerResolverDelegate> HtmlProviders { get; private set; }

        public List<HttpHandlerResolverDelegate> CatchAllHandlers { get; private set; }

        public Dictionary<Type, Func<IHttpRequest, object>> RequestBinders
        {
            get { throw new NotImplementedException(); }
        }

        public EndpointHostConfig Config { get; set; }

        public void RegisterService(Type serviceType, params string[] atRestPaths)
        {
            Config.ServiceManager.RegisterService(serviceType);
        }

        public void LoadPlugin(params IPlugin[] plugins)
        {
            plugins.ToList().ForEach(x => x.Register(this));
        }
    }
}