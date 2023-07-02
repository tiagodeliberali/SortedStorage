namespace SortedStorage.TcpServer;

using System.Collections.Generic;
using System.Net.Sockets;

public class TcpStateObject
{
    public byte[] Buffer { get; private set; }
    public List<byte> ReceivedData { get; }
    public Socket WorkSocket { get; }

    public TcpStateObject(Socket workSocket)
    {
        WorkSocket = workSocket;
        Buffer = new byte[TcpConfiguration.BufferSize];
        ReceivedData = new List<byte>();
    }

    public bool ReceivedAllData() => true;

    public TcpRequest GetRequest() => TcpRequest.FromBytes(ReceivedData.ToArray());

    public TcpResponse GetResponse() => TcpResponse.FromBytes(ReceivedData.ToArray());

    public void Clear()
    {
        Buffer = new byte[TcpConfiguration.BufferSize];
        ReceivedData.Clear();
    }
}
