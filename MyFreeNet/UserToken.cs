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

        public void SetEventArgs(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            receiveArgs.BytesTransferred
        }

        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {

        }
    }
}
