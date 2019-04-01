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
        private TcpClient Client;//hold client socket
        private int Client_Number;//store client number 
        Thread ChThreat;//to keep listening from client
        /// <summary>
        /// initialize client socket and client's number and create thread to listen from client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="number"></param>
        public HandleClients(TcpClient client, int number)
        {
            Client = client;
            Client_Number = number;
            ChThreat = new Thread(Chat);
            ChThreat.Start();//start listening from client
        }
        /// <summary>
        /// broadcasting message retrived from the client to all connected clients
        /// </summary>
        private void Chat()
        {
            int Actual_size = 0;
            int BytesNumber;//hold number of bytes 
            byte[] Buffer;//locate memory to store all bytes received
            string ReceivedData = "";//locate memory to store received data as string 
            while (true)
            {
                try
                {
                    Buffer = new byte[4]; //to store received Data from client
                    NetworkStream network = Client.GetStream();
                    BytesNumber = network.Read(Buffer, 0, Buffer.Length);//save data in buffer and save size of data in BytesNumber
                    if (BytesNumber == 1)//Client closes
                    {
                        byte[] CloseCode = new byte[1];
                        network.Write(CloseCode, 0, 1);//send ACk to client that server will close connection.
                        Console.WriteLine("Client #" + Client_Number + " Closed");
                        Server.clients.Remove(this.Client);//remove client from the list
                        return;
                    }
                    while (Buffer != null)
                    {
                        byte[] Header = new byte[4];
                        Header = Buffer.ToList().GetRange(0,4).ToArray();
                        
                        int Header_size = BitConverter.ToInt32(Header, 0);
                       
                        byte[] packet = new byte[Header_size];
                        if (Buffer.Length==4)
                        {
                            Buffer = new byte[1024 * 20];
                            Actual_size = network.Read(Buffer, 0, Buffer.Length);
                        }
                        else
                        {
                            List<byte> temp = Buffer.ToList();
                            temp.RemoveRange(0, 4);
                            Buffer = temp.ToArray();
                            Actual_size = Buffer.Length;
                        }
                        if (Header_size < Buffer.Length)//cut buffer to actual size
                        {

                            Array.Resize(ref Buffer, Header_size);
                        }
                        if (Header_size == Actual_size)
                        {
                            ReceivedData = Encoding.UTF8.GetString(Buffer);//transform bytes into string
                            Console.WriteLine(ReceivedData);//print string 
                            for (int index = 0; index < Server.clients.Count; index++)//broadcast to all clients 
                            {
                                Server.clients[index].GetStream().Write(Buffer, 0, Buffer.Length);
                            }
                            Buffer = null;
                        }
                        else
                        {
                            packet = Buffer.ToList().GetRange(0, Header_size).ToArray();
                            List<byte> temp = Buffer.ToList();
                            temp.RemoveRange(0, Header_size);
                            Buffer = temp.ToArray();
                            ReceivedData = Encoding.UTF8.GetString(packet);//transform bytes into string
                            Console.WriteLine(ReceivedData);//print string 
                            for (int index = 0; index < Server.clients.Count; index++)//broadcast to all clients 
                            {
                                Server.clients[index].GetStream().Write(packet, 0, packet.Length);
                            }
                        }

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    PrintErrors(ex);
                    Server.clients.Remove(this.Client);
                    return;
                }
            }
        }/// <summary>
         /// Print Errors in Error.txt file in application folder
         /// </summary>
         /// <param name="ex"></param>
        private void PrintErrors(Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;//return path of Application folder
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
