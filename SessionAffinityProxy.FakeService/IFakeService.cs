using System.ServiceModel;

namespace IT.FP.FakeService
{
    /// <summary>
    /// Servizio fake creato per testare il SessionAffinityProxy
    /// </summary>
    [ServiceContract]
    public interface IFakeService
    {
        [OperationContract]
        string GetMessageSeparator();

        [OperationContract]
        string GetHostMessage1();

        [OperationContract]
        string GetHostMessage2();

        [OperationContract]
        string GetHostMessage3();

        [OperationContract]
        string GetRawMessage1();

        [OperationContract]
        string GetRawMessage2();

        [OperationContract]
        string GetRawMessage3();
    }
}
