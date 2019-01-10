using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
namespace SimpleServerCS
{
    class SimpleServer
    {
        public int idGiver = 1;
        private Object thisLock = new Object(); 

        TcpListener _tcpListener;
        static List<Client> _clients = new List<Client>();

        public SimpleServer(string ipAddress, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            _tcpListener = new TcpListener(ip, port);
        }

        public void Start()
        {
            _tcpListener.Start();
            
            Console.WriteLine("Listening...");

            lock (thisLock)
            {
                while (true)
                {
                    Socket socket = _tcpListener.AcceptSocket();
                    Console.WriteLine("Connection Made");
                    Client a = new Client(socket);
                    _clients.Add(a);
                    a.Start();
                    PlayerPacket playerPacket = new PlayerPacket(idGiver);
                    a.Send(playerPacket);
                    idGiver++;
                }
            }
        }

        public void Stop()
        {
            _tcpListener.Stop();
            foreach (Client c in _clients)
            {
                c.Stop();
            }
        }
        
        public static void SocketMethod(Client client)
        {
            Socket socket = client.Socket;

            try
            {

                int noOfIncomingBytes;
                while ((noOfIncomingBytes = client.reader.ReadInt32()) != 0)
                {
                    byte[] bytes = client.reader.ReadBytes(noOfIncomingBytes);
                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    Packet packet = bf.Deserialize(memoryStream) as Packet;

                    switch (packet.type)
                    {
                        case PacketType.CHATMESSAGE:
                            foreach (Client c in _clients)
                            {
                                c.Send(packet);
                            }
                            break;
                        case PacketType.TURN:
                            foreach (Client c in _clients)
                            {
                                c.Send(packet);
                            }
                            break;
                        case PacketType.RESET:
                            foreach (Client c in _clients)
                            {
                                c.Send(packet);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured: " + e.Message);
            }
            finally
            {
                client.Stop();
            }
        }
    }
}
