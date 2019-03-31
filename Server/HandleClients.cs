using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Sockets;
namespace Server
{
    class HandleClients
    {
        private TcpClient Client;
        private int Client_Number;
        public HandleClients (TcpClient client , int number)
        {
            Client = client;
            Client_Number = number;
            Thread ChThreat = new Thread(Chat);
            ChThreat.Start();
        }
        private void Chat ()
        {
            int RequestCount = 0;
            int size;
            Int64 BytesReceived = 0;
            byte[] ReceivedBytes;
            string ReceivedData;
            string[] Client_Info;
            byte[] SendByte = null;
            string SendData="";
            while (true)
            {
                try
                {
                    RequestCount++;
                    ReceivedBytes = new byte[1024 * 20]; ;
                    NetworkStream network = Client.GetStream();
                    network.Read(ReceivedBytes, 0,4);
                    Int64 NumberOfBytes = BitConverter.ToInt64(ReceivedBytes,0);
                    if (NumberOfBytes==0)
                    {
                        Console.WriteLine("Client #" + Client_Number + " Closed");
                        Server.clients.Remove(this.Client);
                        return;
                    }
                    while (BytesReceived<NumberOfBytes &&(size=network.Read(ReceivedBytes,0,ReceivedBytes.Length))>0)
                    {
                        if (size < ReceivedBytes.Length)
                        {
                            Array.Resize(ref ReceivedBytes, size);
                        }
                        ReceivedData = Encoding.ASCII.GetString(ReceivedBytes);
                        if (ReceivedData.Contains('$'))
                        {
                            Client_Info = ReceivedData.Split('$');
                            SendData = Client_Info[0]  + Client_Info[1];
                            Console.WriteLine(SendData);
                        }
                        BytesReceived += size;
                    }
                    BytesReceived = 0;
                    SendByte = Encoding.ASCII.GetBytes(SendData);
                    for (int index = 0; index < Server.clients.Count; index++)
                    {
                        Server.clients[index].GetStream().Write(SendByte, 0, SendByte.Length);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    PrintErrors(ex);
                    Server.clients.Remove(this.Client);
                    return;
                }
            }
        }
        private void PrintErrors(Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt"))
            {
                stream.WriteLine("Date : " + DateTime.Now.ToLocalTime());
                stream.WriteLine("Stack trace :");
                stream.WriteLine(ex.StackTrace);
                stream.WriteLine("Message :");
                stream.WriteLine(ex.Message);
                stream.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
        }
    }
}
