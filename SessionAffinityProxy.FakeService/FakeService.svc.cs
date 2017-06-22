using System.Configuration;
using System.Net;

namespace IT.FP.FakeService
{
    /// <summary>
    /// Servizio fake creato per testare il SessionAffinityProxy
    /// </summary>
    public class FakeService : IFakeService
    {
        string MESSAGESEPARATOR { get { return " :: "; } }
        string MESSAGE1 { get { return ConfigurationManager.AppSettings["message1"]; } }
        string MESSAGE2 { get { return ConfigurationManager.AppSettings["message2"]; } }
        string MESSAGE3 { get { return ConfigurationManager.AppSettings["message3"]; } }

        public string GetMessageSeparator()
        {
            return MESSAGESEPARATOR;
        }

        public string GetHostMessage1()
        {
            return MESSAGE1 + MESSAGESEPARATOR + Dns.GetHostName();
        }

        public string GetHostMessage2()
        {
            return MESSAGE2 + MESSAGESEPARATOR + Dns.GetHostName();
        }

        public string GetHostMessage3()
        {
            return MESSAGE3 + MESSAGESEPARATOR + Dns.GetHostName();
        }

        public string GetRawMessage1()
        {
            return MESSAGE1;
        }

        public string GetRawMessage2()
        {
            return MESSAGE2;
        }

        public string GetRawMessage3()
        {
            return MESSAGE3;
        }

    }
}
