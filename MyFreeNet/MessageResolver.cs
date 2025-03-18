using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFreeNet
{
    public delegate void CompletedMessageCallback(ArraySegment<byte> buffer);

    class Defines
    {
        public static readonly short HEADRSIZE = 4;
    }

    public class MessageResolver
    {
        int _messageSize;
        byte[] _messageBuffer;
        int _currentPosition;
        int _remainBytes;
        int _positionToRead;

        public MessageResolver()
        {
            _messageSize = 0;
            _currentPosition = 0;
            _positionToRead = 0;
            _remainBytes = 0;
        }

        public void OnReceive(byte[] buffer, int offset, int transferred, CompletedMessageCallback callback)
        {
            _remainBytes = transferred;

            int sourcePosition = offset;

            while (_remainBytes > 0)
            {
                bool isCompleted = false;

                if (_currentPosition < Defines.HEADRSIZE)
                {
                    _positionToRead = Defines.HEADRSIZE;

                    isCompleted = _ReadUntil(buffer, ref sourcePosition);

                    if (!isCompleted)
                    {
                        return;
                    }

                    _messageSize = _GetTotalMessageSize();

                    if (_messageSize <= 0)
                    {
                        ClearBuffer();
                        return;
                    }

                    _positionToRead = _messageSize;

                    if (_remainBytes <= 0)
                    {
                        return;
                    }
                }

                isCompleted = _ReadUntil(buffer, ref sourcePosition);

                if (isCompleted)
                {
                    byte[] clone = new byte[_positionToRead];
                    Array.Copy(_messageBuffer, clone, _positionToRead);
                    ClearBuffer();
                    callback(new ArraySegment<byte>(clone, 0, _positionToRead));
                }
            }
        }

        public void ClearBuffer()
        {
            Array.Clear(_messageBuffer, 0, _messageBuffer.Length);
            _currentPosition = 0;
            _messageSize = 0;
        }

        bool _ReadUntil(byte[] buffer, ref int sourcePosition)
        {
            int copySize = _positionToRead - _currentPosition;

            if (_remainBytes < copySize)
            {
                copySize = _remainBytes;
            }

            Array.Copy(buffer, sourcePosition, _messageBuffer, _currentPosition, copySize);

            sourcePosition += copySize;

            _currentPosition += copySize;

            _remainBytes -= copySize;

            if (_currentPosition < _positionToRead)
            {
                return false;
            }

            return true;
        }

        int _GetTotalMessageSize()
        {
            Type type = Defines.HEADRSIZE.GetType();
            if (type.Equals(typeof(Int16)))
            {
                return BitConverter.ToInt16(_messageBuffer, 0);
            }

            return BitConverter.ToInt32(_messageBuffer, 0);
        }
    }
}
