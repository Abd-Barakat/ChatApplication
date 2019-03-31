using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace Server
{
    class Server
    {
        public static List<TcpClient> clients = new List<TcpClient>();
        static void Main(string[] args)
        {
            Console.WriteLine("Server Started");
            IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());
            Console.WriteLine("Ip : "+GetAddress(Host).ToString());
            TcpListener server = new TcpListener( GetAddress(Host) , 5647);
            TcpClient ClientSocket = new TcpClient();
            int ClientsCount = 0;
            server.Start();
            while (true)
            {
                ClientSocket = server.AcceptTcpClient();
                if (clients.Count != 3)
                {
                    string ACK_Msg = "Connection Established";
                    byte[] sendACK = Encoding.ASCII.GetBytes(ACK_Msg);
                    ClientSocket.GetStream().Write(sendACK, 0, sendACK.Length);
                    clients.Add(ClientSocket);
                    ClientsCount=clients.Count;
                    Console.WriteLine("Client #" + ClientsCount.ToString() + " started");
                    HandleClients handle = new HandleClients(ClientSocket, ClientsCount);
                }
                else
                {
                    string Error_Msg = "Server is busy";
                    byte[] sendError = Encoding.ASCII.GetBytes(Error_Msg);
                    ClientSocket.GetStream().Write(sendError, 0, sendError.Length);
                }
            }
        }
        private static IPAddress GetAddress(IPHostEntry Host)
        {
            foreach (IPAddress iP in Host.AddressList)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)
                {
                    return iP;
                }
            }
            return IPAddress.Parse("127.0.0.1");
        }
    }
}
