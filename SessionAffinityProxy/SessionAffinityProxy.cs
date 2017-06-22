using System;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Configuration;
using IT.FP.SessionAffinityProxy.Impl;
using IT.FP.SessionAffinityProxy.Interfaces;


namespace IT.FP.SessionAffinityProxy
{
    /// <summary>
    /// SessionAffinityProxy
    /// </summary>
    public static class ProxyFactory
    {
        //Store delle informazioni
        private static ICacheManager _cacheManagerCache;
        private static IHostInfo _hostInfo = new HostInfo();

        /// <summary>
        /// Sets the cache manager.
        /// L'utilizzo di questo setter è opzionale. 
        /// Di default viene utilizzato un wrapper al metodo del framework Dns.GetHostName()
        /// </summary>
        /// <param name="cacheManger">The cache manger.</param>
        public static void SetCacheManager(ICacheManager cacheManger)
        {
            _cacheManagerCache = cacheManger;
        }

        /// <summary>
        /// Sets the host information.
        /// </summary>
        /// <param name="hostInfo">The host information.</param>
        public static void SetHostInfo(IHostInfo hostInfo)
        {
            _hostInfo = hostInfo;
        }

        /// <summary>
        /// Creates the proxy.
        /// Il metodo considera i parametri endPointName e port solo alla prima chiamata,
        /// dopodichè verrà "semplicemente" utilizzato l'AddressEndpoint creato per ritornare il
        /// proxy corretto
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session">The session.</param>
        /// <param name="endpointName">End name of the point.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static T CreateSessionProxy<T>(string session, string endpointName, int? port = null)
        {
            //Verifico se il gestore cache è settato
            if(_cacheManagerCache == null) throw new ArgumentException(SessionAffinityProxyConst.NOCACHEMANAGER);

            //Istanzio il proxy com dynamic
            dynamic proxy = Activator.CreateInstance<T>();
            
            //Provo a salvare la coppia session-endpoint
            SaveSessionRelatedEndpointAddressOnlyIfFirstCall(session, endpointName, port);

            //Recupero l'EndpointAddress annesso alla sessione
            proxy.Endpoint.Address = GetSessionRelatedEndpointAddress(session);

            //Ritorno il dato
            return proxy;
        }

        /// <summary>
        /// Creates the session proxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TP">The type of the p.</typeparam>
        /// <param name="session">The session.</param>
        /// <param name="endpointName">Name of the endpoint.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static T CreateSessionProxy<T, TP>(string session, string endpointName, int? port = null) where TP : ICacheManager
        {
            SetCacheManager(Activator.CreateInstance<TP>());
            return CreateSessionProxy<T>(session, endpointName, port);
        }

        /// <summary>
        /// Removes the session proxy.
        /// </summary>
        /// <param name="session">The session.</param>
        public static void RemoveSessionProxy(string session)
        {
            //Verifico se il gestore cache è settato
            if (_cacheManagerCache == null) throw new ArgumentException(SessionAffinityProxyConst.NOCACHEMANAGER);

            _cacheManagerCache.Remove(session);
        }

        /// <summary>
        /// Gets the session related endpoint address.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        private static EndpointAddress GetSessionRelatedEndpointAddress(string session)
        {
            return (EndpointAddress)_cacheManagerCache.Get(session);
        }

        /// <summary>
        /// Saves the session related endpoint address only if first call.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="endpointName">End name of the point.</param>
        /// <param name="port">The port.</param>
        private static void SaveSessionRelatedEndpointAddressOnlyIfFirstCall(string session, string endpointName, int? port)
        {
            //Se esiste già un elemento associato alla sessione ritorno
            object tmp;
            if (_cacheManagerCache.TryGet(session, out tmp)) return;

            //Se non esiste un elemento associato alla sessione, lo creo e lo inserisco nello store
            //Prima chiamata
            _cacheManagerCache.Put(session, LocalizeMachineEndpointAddress(endpointName, port));
        }

        /// <summary>
        /// Localizes the machine endpoint address.
        /// </summary>
        /// <param name="endpointName">End name of the point.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        private static EndpointAddress LocalizeMachineEndpointAddress(string endpointName, int? port)
        {
            int endPointPort;
            string serviceLocalAddress, protocol;
            
            //Dall'epoint originale ottengo le informazioni per costruire il nuovo endpoint con address locale
            RetriveEndpointElements(endpointName, out protocol, out endPointPort, out serviceLocalAddress);
            if (port != null) endPointPort = port.Value;
            return new EndpointAddress(new UriBuilder(protocol, _hostInfo.GetServiceAddress(), endPointPort, serviceLocalAddress).ToString());
        }

        /// <summary>
        /// Retrives the endpoint elements.
        /// </summary>
        /// <param name="endpointName">End name of the point.</param>
        /// <param name="endpointProtocol">The end point protocol.</param>
        /// <param name="endpointPort">The end point port.</param>
        /// <param name="serviceLocalAddress">The service local address.</param>
        /// <exception cref="System.ServiceModel.EndpointNotFoundException">
        /// </exception>
        private static void RetriveEndpointElements(string endpointName, out string endpointProtocol, out int endpointPort, out string serviceLocalAddress)
        {
            //Recupero le informazioni dell'Endpoint dal web.config/app.config
            var clientSection = ConfigurationManager.GetSection(SessionAffinityProxyConst.ENDPOINTSECTION) as ClientSection;

            //Verifico che esite almeno un endPoint
            if (clientSection == null || clientSection.Endpoints == null)
                throw new EndpointNotFoundException(SessionAffinityProxyConst.NOENDPOINTFOUND);

            //Recupero l'endPoint specifico e verifico se effettivamente esiste
            var selectedEndPoint = clientSection.Endpoints.OfType<ChannelEndpointElement>().
                FirstOrDefault(p => p.Name == endpointName);

            if (selectedEndPoint == null)
                throw new EndpointNotFoundException(SessionAffinityProxyConst.ENDPOINTFOUND);

            //Assegno i valori in out
            endpointProtocol = selectedEndPoint.Address.Scheme;
            endpointPort = selectedEndPoint.Address.Port;
            serviceLocalAddress = selectedEndPoint.Address.LocalPath;
        }
    }
}
