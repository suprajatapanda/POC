namespace TRS.IT.TAEMQCon.mqSvc;
[System.ServiceModel.ServiceContract(Namespace = "http://transamerica.com/LosAngeles/")]
public interface IMQService
{
    [System.ServiceModel.OperationContract(Action = "http://transamerica.com/LosAngeles/SubmitTransaction",
        ReplyAction = "http://transamerica.com/LosAngeles/SubmitTransactionResponse")]
    string SubmitTransaction(string msg, int transTypeID, string sMachineIp, string sClientIp, string sSessionId);

    [System.ServiceModel.OperationContract(Action = "http://transamerica.com/LosAngeles/SubmitTransaction",
        ReplyAction = "http://transamerica.com/LosAngeles/SubmitTransactionResponse")]
    Task<string> SubmitTransactionAsync(string msg, int transTypeID, string sMachineIp, string sClientIp, string sSessionId);
}

// WCF Client implementation
public class MQService : IDisposable
{
    private readonly System.ServiceModel.ChannelFactory<IMQService> _channelFactory;
    private readonly IMQService _channel;
    private bool _disposed;

    public MQService() : this("http://localhost/trsservices/mqservice.asmx")
    {
    }

    public MQService(string endpointUrl)
    {
        var binding = new System.ServiceModel.BasicHttpBinding
        {
            MaxReceivedMessageSize = 65536,
            MaxBufferSize = 65536,
            SendTimeout = TimeSpan.FromMinutes(1),
            ReceiveTimeout = TimeSpan.FromMinutes(1)
        };

        var endpoint = new System.ServiceModel.EndpointAddress(endpointUrl);
        _channelFactory = new System.ServiceModel.ChannelFactory<IMQService>(binding, endpoint);
        _channel = _channelFactory.CreateChannel();
    }

    public string SubmitTransaction(string msg, int transTypeID, string sMachineIp, string sClientIp, string sSessionId)
    {
        return _channel.SubmitTransaction(msg, transTypeID, sMachineIp, sClientIp, sSessionId);
    }

    public Task<string> SubmitTransactionAsync(string msg, int transTypeID, string sMachineIp, string sClientIp, string sSessionId)
    {
        return _channel.SubmitTransactionAsync(msg, transTypeID, sMachineIp, sClientIp, sSessionId);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                try
                {
                    if (_channel != null)
                    {
                        var clientChannel = _channel as System.ServiceModel.IClientChannel;
                        if (clientChannel?.State == System.ServiceModel.CommunicationState.Faulted)
                        {
                            clientChannel.Abort();
                        }
                        else
                        {
                            clientChannel?.Close();
                        }
                    }
                    _channelFactory?.Close();
                }
                catch
                {
                    _channelFactory?.Abort();
                }
            }
            _disposed = true;
        }
    }
}