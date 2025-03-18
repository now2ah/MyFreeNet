using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyFreeNet
{
    public class UserToken
    {
        enum State
        {
            IDLE,
            CONNECTED,
            RESERVECLOSING,
            CLOSED
        }

        public Socket socket;
        public SocketAsyncEventArgs receiveEventArgs { get; private set; }
        public SocketAsyncEventArgs sendEventArgs { get; private set; }

        MessageResolver _messageResolver;
        object _sendingQueueLockObject;
        List<ArraySegment<byte>> _sendingList;
        State currentState;

        public void SetEventArgs(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            
        }

        public void Send(ArraySegment<byte> data)
        {
            lock (_sendingQueueLockObject)
            {
                _sendingList.Add(data);

                if (_sendingList.Count > 1)
                {
                    return;
                }
            }

            _StartSend();
        }

        public void Send(Packet packet)
        {
            packet.RecordSize();
            Send(new ArraySegment<byte>(packet.buffer, 0, packet.position));
        }

        public void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success)
            {
                return;
            }

            lock(_sendingQueueLockObject)
            {
                //total bytes
                var size = _sendingList.Sum(obj => obj.Count);

                if (e.BytesTransferred != size)
                {
                    if (e.BytesTransferred < _sendingList[0].Count)
                    {
                        Console.WriteLine("error : need to send more");
                        Close();
                        return;
                    }

                    int sentIndex = 0;
                    int sum = 0;
                    for (int i=0; i<_sendingList.Count; ++i)
                    {
                        sum += _sendingList[i].Count;
                        if (sum <= e.BytesTransferred)
                        {
                            sentIndex = i;
                            continue;
                        }
                        break;
                    }

                    _sendingList.RemoveRange(0, sentIndex + 1);

                    _StartSend();
                    return;
                }

                _sendingList.Clear();

                if (currentState == State.RESERVECLOSING)
                {
                    socket.Shutdown(SocketShutdown.Send);
                }
            }
        }

        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            _messageResolver.OnReceive(buffer, offset, bytesTransferred, _OnMessageCompleted);
        }

        public void Close()
        {

        }

        void _OnMessageCompleted(ArraySegment<byte> buffer)
        {

        }

        void _StartSend()
        {
            try
            {
                sendEventArgs.BufferList = _sendingList;

                bool pending = socket.SendAsync(sendEventArgs);
                if (!pending)
                {
                    ProcessSend(sendEventArgs);
                }
            }
            catch (Exception e)
            {
                if (null == socket)
                {
                    Close();
                    return;
                }

                Console.WriteLine("send error : " + e.Message);
                throw new Exception(e.Message, e);
            }
        }
    }
}
