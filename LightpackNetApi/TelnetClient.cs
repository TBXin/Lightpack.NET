using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace LightpackNetApi
{
    class TelnetClient
    {
        private Socket socket;

        private readonly string serverAddress;
        private readonly int serverPort;
        private readonly byte[] receiveBuffer;

        public Boolean IsConnected
        {
            get
            {
                if (socket == null)
                    return false;

                var canRead = socket.Poll(1000, SelectMode.SelectWrite);
                return canRead;
            }
        }

        public TelnetClient(string server, int port)
        {
            serverAddress = server;
            serverPort = port;
            receiveBuffer = new byte[8192];
        }

        public void Connect()
        {
            if (IsConnected)
                throw new InvalidOperationException("Already connected");

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(serverAddress, serverPort);
            socket.Receive(receiveBuffer);
        }

        public void Disconnect()
        {
            if (!IsConnected)
                return;

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            socket.Close();
            socket = null;
        }

        public void Write(string command)
        {
            if(!IsConnected)
                throw new InvalidOperationException("Not connected");

            var commandBytes = Encoding.UTF8.GetBytes(command + "\r\n");
            socket.Send(commandBytes);
        }

        public string Read()
        {
            var bytesReceived = socket.Receive(receiveBuffer);
            var response = Encoding.UTF8.GetString(receiveBuffer.Take(bytesReceived).ToArray());
            return response;
        }
    }
}
