using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
namespace Server
{
    class HandleClients
    {
        private TcpClient Client;
        private int Number;
        public HandleClients (TcpClient client , int number)
        {
            Client = client;
            Number = number;
            Thread ChThreat = new Thread(Chat);
            ChThreat.Start();
        }
        private void Chat ()
        {
            int RequestCount = 0;
            byte[] receivedData = new byte[20000];
            byte[] SendByte = null;
            while (true)
            {
                try
                {
                    RequestCount++;
                    NetworkStream network = Client.GetStream();
                    int size = network.Read(receivedData, 0, receivedData.Length);
                    if (size <receivedData.Length)
                    {
                        Array.Resize(ref receivedData, size);
                    }
                    string ReceivedData = Encoding.ASCII.GetString(receivedData);
                    Console.WriteLine("Data from client #" + Number.ToString() + ReceivedData);
                    string SendData = "Server to client #" + Number.ToString() + " response #" + RequestCount.ToString();
                    SendByte = Encoding.ASCII.GetBytes(SendData);
                    network.Write(SendByte, 0, SendByte.Length);
                    Console.WriteLine(SendData);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }
    }
}
