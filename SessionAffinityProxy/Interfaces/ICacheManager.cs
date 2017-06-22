namespace IT.FP.SessionAffinityProxy.Interfaces
{
    public interface ICacheManager
    {
        void Put(string key, object value);
        object Get(string key);
        bool TryGet(string key, out object value);
        void Remove(string idOperazioneCanale);
    }
}
