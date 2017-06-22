using System.Net;
using IT.FP.SessionAffinityProxy.Interfaces;

namespace IT.FP.SessionAffinityProxy.Impl
{
    /// <summary>
    /// HostInfo
    /// </summary>
    public class HostInfo : IHostInfo
    {
        /// <summary>
        /// Gets the service address.
        /// </summary>
        /// <returns></returns>
        public string GetServiceAddress()
        {
            return Dns.GetHostName();
        }
    }
}
