using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SortedStorage.TcpServer
{
    public abstract class SocketListener
    {
        public ManualResetEvent allDone = new ManualResetEvent(false);
                
        public SocketListener()
        {
        }

        public void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(SocketListener)}] {e}");
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            TcpServiceEventSource.Log.IncrementActiveClients();
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            TcpStateObject state = new TcpStateObject(handler);
            state.WorkSocket.BeginReceive(state.Buffer, 0, TcpStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public async void ReadCallback(IAsyncResult ar)
        {
            TcpStateObject state = (TcpStateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;

            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                TcpServiceEventSource.Log.IncrementReadBytes(bytesRead);
                state.ReceivedData.AddRange(state.Buffer.Take(bytesRead));

                if (state.ReceivedAllData())
                {
                    TcpResponse response = await ProcessRequest(state.GetRequest());
                    state.Clear();

                    Send(handler, response.ToBytes());
                }

                handler.BeginReceive(state.Buffer, 0, TcpStateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            }
        }

        protected abstract Task<TcpResponse> ProcessRequest(TcpRequest tcpRequest);

        private void Send(Socket handler, IEnumerable<byte> data) => 
            handler.BeginSend(data.ToArray(), 0, data.Count(), 0, new AsyncCallback(SendCallback), handler);

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                TcpServiceEventSource.Log.IncrementSentBytes(bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{nameof(SocketListener)}] {e}");
            }
        }
    }
}
