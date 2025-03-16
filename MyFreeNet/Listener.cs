using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace MyFreeNet
{
    public class Listener
    {
        //비동기 Accept EventArgs
        SocketAsyncEventArgs acceptArgs;

        //클라이언트 접속 Socket
        Socket listenSocket;

        //Accept 처리 순서 제어
        AutoResetEvent flowControlEvent;

        //새 클라이언트 접속 delegate
        public delegate void NewClientHandler(Socket clientSocket, object token);
        public NewClientHandler callbackOnNewClient;

        public Listener()
        {
            callbackOnNewClient = null;
        }

        public void Start(string hostIPString, int portNumber, int backLog)
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress;
            if (hostIPString == "0.0.0.0")
            {
                ipAddress = IPAddress.Any;
            }
            else
            {
                ipAddress = IPAddress.Parse(hostIPString);
            }

            IPEndPoint endPoint = new IPEndPoint(ipAddress, portNumber);

            try
            {
                listenSocket.Bind(endPoint);
                listenSocket.Listen(backLog);

                acceptArgs = new SocketAsyncEventArgs();
                acceptArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

                Thread listenThread = new Thread(DoListen);
                listenThread.Start();

                //listenSocket.AcceptAsync(acceptArgs);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket clientSocket = e.AcceptSocket;

                flowControlEvent.Set();

                if (callbackOnNewClient != null)
                {
                    callbackOnNewClient(clientSocket, e.UserToken);
                }
            }
        }

        void DoListen()
        {
            flowControlEvent = new AutoResetEvent(false);

            while (true)
            {
                acceptArgs.AcceptSocket = null;

                bool pending = true;
                try
                {
                    pending = listenSocket.AcceptAsync(acceptArgs);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                //accept 동기적으로 처리
                if (!pending)
                {
                    OnAcceptCompleted(null, acceptArgs);
                }

                flowControlEvent.WaitOne();
            }
        }
    }
}
