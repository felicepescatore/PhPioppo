namespace IT.FP.SessionAffinityProxy.Impl
{
    /// <summary>
    /// SessionAffinityProxyConst
    /// </summary>
    static class SessionAffinityProxyConst
    {
        public const string NOENDPOINTFOUND = "Nessun Endpoint trovato nel file di configurazione";
        public const string ENDPOINTFOUND = "Endpoint specificato non trovato";
        public const string ENDPOINTSECTION = "system.serviceModel/client";
        public const string NOCACHEMANAGER =
            "Gestore della cache non settato. Si prega di utilizzate prima SetCacheManager";
    }
}
