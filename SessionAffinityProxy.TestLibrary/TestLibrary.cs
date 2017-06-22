using System;
using System.Configuration;
using System.ServiceModel;
using IT.FP.Impl;
using IT.FP.SessionAffinityProxy;
using IT.FP.TestLibrary.FakesImpl;
using IT.FP.TestLibrary.FakeService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IT.FP.TestLibrary
{
    [TestClass]
    public class TestLibrary
    {
        /// <summary>
        /// CreateProxyWithoutInitCacheManager.
        /// Questo test ha lo scopo di verificare che venga sollevata un'eccezione qualora non si settato
        /// il cache manager
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateProxyWithoutInitCacheManager()
        {
            //Creo un proxy senza inizializzare il cacheManager in modo diretto:
            //  1) SessionAffinityProxy.SetCacheManager(cacheManager);
            //  2) var client = SessionAffinityProxy.CreateSessionProxy<Client>(session, endPointName);
            // o indiretto (da usare per una sola chiamata)
            //  1) var client = SessionAffinityProxy.CreateSessionProxy<Client, CacheManager>(session, endPointName);
            ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME, TestLibraryConstant.RIGHTPORT);
        }

        /// <summary>
        /// CreateProxyForLocalhostAndRightPort.
        /// Questo test verifica la creazione del proxy e l'invocazione delle operation annesse al servizio
        /// </summary>
        [TestMethod]
        public void CreateProxyForLocalhostAndRightPort()
        {
            //Setto l'oggeto che si occupa di ritornare le informazioni DNS di invocazione
            //In questo caso utilizzoun Fake che, alternatamente, mi ritorna le impostazioni
            //presenti nell'app/web.config : address1 e address2 in modo da poter fare i test su due
            //macchine differenti
            ProxyFactory.SetHostInfo(new FakeHostInfo());

            //Creo un proxy per la chiamata alla prima operation, con RIGHTPORT (default) e lo associo alle sessione SESSION1
            //L'utilizzo di FakeHostInfo come HostInfo mi garantisce che, essendo il primo utilizzo implicito, mi ritorni come address:Address1
            //Nel caso di una sola invocazione è possibile utilizzare il CreateSessioneProxy<Client,Cache> invece che inizializzare prima la cache e poi procedere alle invocazioni
            var client = ProxyFactory.CreateSessionProxy<FakeServiceClient, FakeCacheManager>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME, TestLibraryConstant.RIGHTPORT);

            //Verifico le condizioni di Assert
            Assert.AreEqual(client.GetHostMessage1(), client.GetRawMessage1() + client.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);
            Assert.AreEqual(client.GetHostMessage2(), client.GetRawMessage2() + client.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);
            Assert.AreEqual(client.GetHostMessage3(), client.GetRawMessage3() + client.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);
        }

        /// <summary>
        /// Create2ProxyForLocalhostAndRightPort.
        /// Questo test ha lo scopo di dimostrare che l'address viene settato solo alla prima invocazione
        /// per la creazione del proxy associato alla specifica sessione.
        /// Nel caso di richiesta di un secondo proxy per la stessa sessione, viene ritornato il valore dell'address
        /// precedente, ignorando i parametri passati.
        /// </summary>
        [TestMethod]
        public void Create2ProxyForLocalhostAndRightPort()
        {
            //Setto l'oggeto che si occupa di ritornare le informazioni DNS di invocazione
            //In questo caso utilizzoun Fake che, alternatamente, mi ritorna le impostazioni
            //presenti nell'app/web.config : address1 e address2 in modo da poter fare i test su due
            //macchine differenti
            ProxyFactory.SetHostInfo(new FakeHostInfo());

            //Setto il gestore della cache - Pre condizione per la prima invocazione in assoluto del SessionAffinityProxy
            ProxyFactory.SetCacheManager(new FakeCacheManager());

            //Creo un proxy per la chiamata alla prima operation, con RIGHTPORT (default) e lo associo alle sessione SESSION1
            //L'utilizzo di FakeHostInfo come HostInfo mi garantisce che, essendo il primo utilizzo implicito, mi ritorni come address:Address1
            var client = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME);
            Assert.AreEqual(client.GetHostMessage1(), client.GetRawMessage1() + client.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);

            //Creo un secondo proxy per la stessa sessione.
            //Mi aspetto che la WRONGPORT, così come da design del SessionAffinityProxy, venga ignorata perchè per la sessione
            //è già stato creato un proxy che deve essermi ritornato nello stesso modo per tutte le chiamate.
            var clientB = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME, TestLibraryConstant.WRONGPORT);

            //Verifico le condizioni di Assert
            Assert.AreNotEqual(clientB.Endpoint.Address.Uri.Port, TestLibraryConstant.WRONGPORT);
            Assert.AreEqual(clientB.Endpoint.Address.Uri.Port, client.Endpoint.Address.Uri.Port);
            Assert.AreEqual(clientB.GetHostMessage2(), clientB.GetRawMessage2() + clientB.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);
        }


        /// <summary>
        /// Create2ProxyForLocalhostAndRightPortAndDifferentSession.
        /// Questo test ha lo scopo di verificare la corretta creazione di due proxy differenti
        /// associati a sessioni differenti.
        /// L'utilizzo del FakeHostInfo garantisce che le macchine (DNS) di riferimento per l'address
        /// siano diverse, in funzione di quanto configurato nell'App.config
        /// </summary>
        [TestMethod]
        public void Create2ProxyForLocalhostAndRightPortAndDifferentSession()
        {
            //Setto l'oggeto che si occupa di ritornare le informazioni DNS di invocazione
            //In questo caso utilizzoun Fake che, alternatamente, mi ritorna le impostazioni
            //presenti nell'app/web.config : address1 e address2 in modo da poter fare i test su due
            //macchine differenti
            ProxyFactory.SetHostInfo(new FakeHostInfo());
            
            //Setto il gestore della cache - Pre condizione per la prima invocazione in assoluto del SessionAffinityProxy
            ProxyFactory.SetCacheManager(new FakeCacheManager());

            //Creo un proxy per la chiamata alla prima operation, con RIGHTPORT e lo associo alle sessione SESSION1
            //L'utilizzo di FakeHostInfo come HostInfo mi garantisce che, essendo il primo utilizzo implicito, mi ritorni come address:Address1
            var clientA = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME);

            //Creo un proxy per la chiamata alla prima operation, con RIGHTPORT e lo associo alle sessione SESSION2
            //L'utilizzo di FakeHostInfo come HostInfo mi garantisce che, essendo il secondo utilizzo implicito, mi ritorni come address:Address2
            var clientB = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION2, TestLibraryConstant.ENDPOINTNAME);

            //Verifico le condizioni di Assert
            Assert.AreEqual(clientA.GetHostMessage1(), clientA.GetRawMessage1() + clientA.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);
            Assert.AreEqual(clientB.GetHostMessage2(), clientB.GetRawMessage2() + clientA.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS2]);
        }

        /// <summary>
        /// CreateProxyForLocalhostAndPortRightPortCleanAndRecreate.
        /// Questo test ha lo scopo di verificare la corretta rimozione delle informazioni associate a una sessione.
        /// Viene prima creato un proxy, poi invocato il servizio, quindi vengono rimosse le informazioni.
        /// A questo punto viene creato un nuovo proxy per la stessa sessione e si verifica che i parametri passati
        /// vengano correttamente utilizzati e non ignorati.
        /// Esseno la porta del secondo proxy errata WRONGPORT, ci si attende un'eccezione EndpointNotFoundException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EndpointNotFoundException))]
        public void CreateProxyForLocalhostAndPortRightPortCleanAndRecreate()
        {
            //Setto l'oggeto che si occupa di ritornare le informazioni DNS di invocazione
            //In questo caso utilizzoun Fake che, alternatamente, mi ritorna le impostazioni
            //presenti nell'app/web.config : address1 e address2 in modo da poter fare i test su due
            //macchine differenti
            ProxyFactory.SetHostInfo(new FakeHostInfo());

            //Setto il gestore della cache - Pre condizione per la prima invocazione in assoluto del SessionAffinityProxy
            ProxyFactory.SetCacheManager(new FakeCacheManager());

            //Creo un proxy per la chiamata alla prima operation, con RIGHTPORT e lo associo alle sessione SESSION1
            //L'utilizzo di FakeHostInfo come HostInfo mi garantisce che, essendo il primo utilizzo implicito, mi ritorni come address:Address1
            var clientA = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME);
            Assert.AreEqual(clientA.GetHostMessage1(), clientA.GetRawMessage1() + clientA.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS1]);

            //Rimuovo le informazioni associate alla sessione
            ProxyFactory.RemoveSessionProxy(TestLibraryConstant.SESSION1);

            //Creo un secondo proxy per la stessa sessione, dopo aver rimosso le info.
            //Mi aspetto che la WRONGPORT venga utilizzata effettivamente dal nuovo proxy, sollevando un'eccezzione di
            //tipo "EndpointNotFoundException" all'atto di invocazione del sevizio.
            var clientB = ProxyFactory.CreateSessionProxy<FakeServiceClient>(TestLibraryConstant.SESSION1, TestLibraryConstant.ENDPOINTNAME, TestLibraryConstant.WRONGPORT);

            //Verifico le condizioni di Assert
            Assert.AreEqual(clientB.Endpoint.Address.Uri.Port, TestLibraryConstant.WRONGPORT);
            Assert.AreNotEqual(clientB.Endpoint.Address.Uri.Port, clientA.Endpoint.Address.Uri.Port);
            Assert.AreEqual(clientB.GetHostMessage2(), clientA.GetRawMessage2() + clientA.GetMessageSeparator() + ConfigurationManager.AppSettings[TestLibraryConstant.ADDRESS2]);
        }
    }
}
