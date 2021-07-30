using SortedStorage.TcpServer;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SortedStorage.TcpClient
{
    public class AsynchronousClient
    {
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private Socket client;
        private TcpResponse response;

        public void StartClient(IPAddress ipAddress)
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, TcpConfiguration.ServicePort);

                client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback),
                    client);

                connectDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(AsynchronousClient)}] {e}");
            }
        }

        public TcpResponse Send(TcpRequest request)
        {
            response = null;
            sendDone.Reset();
            receiveDone.Reset();

            Send(request.ToBytes());
            sendDone.WaitOne();

            Receive();
            receiveDone.WaitOne();

            return response;
        }

        public void Close()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(AsynchronousClient)}] {e}");
            }
        }

        private void Send(byte[] data) => client.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), client);

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                int bytesSent = client.EndSend(ar);
                Console.WriteLine($"[{nameof(AsynchronousClient)}] Bytes sent: {bytesSent}");

                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(AsynchronousClient)}] {e}");
            }
        }

        private void Receive()
        {
            try
            {
                TcpStateObject state = new TcpStateObject(client);
                client.BeginReceive(state.Buffer, 0, TcpConfiguration.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(AsynchronousClient)}] {e}");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                TcpStateObject state = (TcpStateObject)ar.AsyncState;
                Socket client = state.WorkSocket;

                int bytesRead = client.EndReceive(ar);
                Console.WriteLine($"[{nameof(AsynchronousClient)}] Bytes received: {bytesRead}");
                if (bytesRead > 0)
                {
                    state.ReceivedData.AddRange(state.Buffer.Take(bytesRead));

                    if (state.ReceivedAllData())
                    {
                        response = state.GetResponse();
                        receiveDone.Set();
                    }
                    else
                    {
                        client.BeginReceive(state.Buffer, 0, TcpConfiguration.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(AsynchronousClient)}] {e}");
            }
        }
    }
}
