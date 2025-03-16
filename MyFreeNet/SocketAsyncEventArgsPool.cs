using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace MyFreeNet
{
    public class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> pool;

        public SocketAsyncEventArgsPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        public int Count { get { return pool.Count; } }

        public void Push(SocketAsyncEventArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException("SocketAsyncEventArgs is null : (Pushing pool)");
            }

            lock (pool)
            {
                pool.Push(args);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                return pool.Pop();
            }
        }
    }
}
