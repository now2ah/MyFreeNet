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
        public Socket socket;
        MessageResolver _messageResolver;

        public void SetEventArgs(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            
        }

        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            _messageResolver.OnReceive(buffer, offset, bytesTransferred, _OnMessageCompleted);
        }

        void _OnMessageCompleted(ArraySegment<byte> buffer)
        {

        }
    }
}
