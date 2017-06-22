using System.Collections.Generic;
using IT.FP.SessionAffinityProxy.Interfaces;

namespace IT.FP.TestLibrary.FakesImpl
{
    /// <summary>
    /// FakeCacheManager
    /// </summary>
    public class FakeCacheManager : ICacheManager
    {
        private static readonly Dictionary<string, object> _cache = new Dictionary<string, object>();

        public void Put(string key, object value)
        {
            _cache.Add(key, value);
        }

        public object Get(string key)
        {
            return _cache[key];
        }

        public bool TryGet(string key, out object value)
        {
            if (_cache.ContainsKey(key))
            {
                value = _cache[key];
                return true;
            }

            value = null;
            return false;

        }

        public void Remove(string idOperazioneCanale)
        {
            if (_cache.ContainsKey(idOperazioneCanale))
            _cache.Remove(idOperazioneCanale);
        }
    }
}
