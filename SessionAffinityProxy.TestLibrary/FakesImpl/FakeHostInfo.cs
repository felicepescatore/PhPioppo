using System;
using System.Configuration;
using IT.FP.SessionAffinityProxy.Interfaces;

namespace IT.FP.TestLibrary.FakesImpl
{
    /// <summary>
    /// FakeHostInfo
    /// </summary>
    public class FakeHostInfo : IHostInfo
    {
        /// <summary>
        /// Gets the service address.
        /// </summary>
        /// <returns></returns>
        public string GetServiceAddress()
        {
            return FakeDns.GetHostName();
        }

        /// <summary>
        /// FakeDns
        /// </summary>
        private static class FakeDns
        {
            private static bool _first;
            private static string _address1;
            private static string _address2;

            static FakeDns()
            {
                _first = true;

                _address1 = ConfigurationManager.AppSettings["address1"];
                _address2 = ConfigurationManager.AppSettings["address2"];
                
                if (_address1 == null ||_address2 == null) 
                    throw new Exception("address1 o address2 non settatti nell'app/web config");
            }
            public static string GetHostName()
            {
                if (_first)
                {
                    _first = false;
                    return _address1;
                }
                _first = true;
                return _address2;
            }
        }
    }
}
