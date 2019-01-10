using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Shared;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleServerCS
{

    class Client
    {
        public Socket Socket;
        public string ID;
        private Thread thread;

        public NetworkStream stream;
        public BinaryReader reader;
        public BinaryWriter writer;

        public Client(Socket socket)
        {
            ID = Guid.NewGuid().ToString();
            Socket = socket;

            stream = new NetworkStream(socket, true);
            reader = new BinaryReader(stream, Encoding.UTF8);
            writer = new BinaryWriter(stream, Encoding.UTF8);
        }

        public void Start()
        {
            thread = new Thread(new ThreadStart(SocketMethod));
            thread.Start();
        }

        public void Stop()
        {
            Socket.Close();
            if (thread.IsAlive)
            {
                thread.Abort();
            }
        }
        public void SocketMethod()
        {
            SimpleServer.SocketMethod(this);
        }

        public void SendText(String message)
        {
            writer.Write(message);
            writer.Flush();
        }

        public void Send(Packet packet)
        {
            MemoryStream mem = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mem, packet);
            byte[] buffer = mem.GetBuffer();

            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }
    }
}
