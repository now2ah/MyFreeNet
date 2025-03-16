using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace MyFreeNet
{
    public class NetworkService
    {
        //클라이언트 접속 Listener
        //Listener clientListener;

        //메시지 수신, 전송 pool
        SocketAsyncEventArgsPool receiveEventArgsPool;
        SocketAsyncEventArgsPool sendEventArgsPool;

        //소켓 버퍼 관리 매니저
        //BufferManager bufferManager;

        public delegate void SessionHandler(UserToken userToken);
        public SessionHandler sessionCreatedCallback { get; set; }

        public void Initialize()
        {
            int maxConnections = 10000;
            int bufferSize = 1024;
            Initialize(maxConnections, bufferSize);
        }

        public void Initialize(int maxConnections, int bufferSize)
        {
            int preAllocationCount = 1;

            BufferManager bufferManager;
            bufferManager = new BufferManager(maxConnections * bufferSize * preAllocationCount, bufferSize);

            receiveEventArgsPool = new SocketAsyncEventArgsPool(maxConnections);
            sendEventArgsPool = new SocketAsyncEventArgsPool(maxConnections);

            SocketAsyncEventArgs arg;

            for (int i=0; i<maxConnections; i++)
            {
                UserToken userToken = new UserToken();
                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(_ReceiveCompleted);
                    arg.UserToken = userToken;

                    bufferManager.SetBuffer(arg);

                    receiveEventArgsPool.Push(arg);
                }

                {
                    arg = new SocketAsyncEventArgs();
                    arg.Completed += new EventHandler<SocketAsyncEventArgs>(_SendCompleted);
                    arg.UserToken = userToken;

                    //send 버퍼 설정, BufferList 사용
                    bufferManager.SetBuffer(arg);

                    sendEventArgsPool.Push(arg);
                }
            }
        }

        public void Listen(string hostString, int portNumber, int backLog)
        {
            Listener listener = new Listener();
            listener.callbackOnNewClient += _OnNewClient;
            listener.Start(hostString, portNumber, backLog);
        }

        void _OnNewClient(Socket clientSocket, object token)
        {
            SocketAsyncEventArgs receiveArgs = receiveEventArgsPool.Pop();
            SocketAsyncEventArgs sendArgs = sendEventArgsPool.Pop();

            if (sessionCreatedCallback != null)
            {
                UserToken userToken = receiveArgs.UserToken as UserToken;
                sessionCreatedCallback(userToken);
            }

            _BeginReceive(clientSocket, receiveArgs, sendArgs);
        }

        void _BeginReceive(Socket socket, SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            UserToken token = receiveArgs.UserToken as UserToken;
            token.SetEventArgs(receiveArgs, sendArgs);

            token.socket = socket;

            bool pending = socket.ReceiveAsync(receiveArgs);

            if (!pending)
            {
                _ProcessReceive(receiveArgs);
            }
        }

        void _ProcessReceive(SocketAsyncEventArgs e)
        {
            UserToken token = e.UserToken as UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                token.OnReceive(e.Buffer, e.Offset, e.BytesTransferred);

                bool pending = token.socket.ReceiveAsync(e);
                if (!pending)
                {
                    _ProcessReceive(e);
                }
            }
            else
            {
                Console.WriteLine(string.Format("error {0}, transferred {1}", e.SocketError, e.BytesTransferred));
                _CloseClientSocket(token);
            }
        }

        void _ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                _ProcessReceive(e);
                return;
            }

            throw new ArgumentException("last operation not a receive");
        }

        void _SendCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        void _CloseClientSocket(UserToken token)
        {

        }
    }
}
