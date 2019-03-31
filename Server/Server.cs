﻿using System;
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
        public static List<TcpClient> clients = new List<TcpClient>();//hold clients in list (create group of clients).
        /// <summary>
        /// Initialize server & create thread to listen from server
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("Server Started");
            IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());//get host 
            Console.WriteLine("Ip : "+GetAddress(Host).ToString());//get server address 
            TcpListener server = new TcpListener( GetAddress(Host) , 5647);//create socket to listen at server at given port number
            TcpClient ClientSocket ;//to hold client socket
            int ClientNumber = 0;
            server.Start();//start listening for client's request
            while (true)
            {
                ClientSocket = server.AcceptTcpClient();//accept client 
                if (clients.Count != 3)//check if the group is full
                {
                    string ACK_Msg = "Connection Established";
                    byte[] sendACK = Encoding.ASCII.GetBytes(ACK_Msg);//return ACK_Msg bytes
                    ClientSocket.GetStream().Write(sendACK, 0, sendACK.Length);//write on client stream (send ack to client).
                    clients.Add(ClientSocket);//add the client to the group
                    ClientNumber=clients.Count;//assign Client number to Number of clients in clients list
                    Console.WriteLine("Client #" + ClientNumber.ToString() + " started");
                    HandleClients handle = new HandleClients(ClientSocket, ClientNumber);//handle client 
                }
                else
                {
                    byte[] sendError = new byte[2];
                    ClientSocket.GetStream().Write(sendError, 0, sendError.Length);//send two bytes to clients to indecate that room is full
                    ClientSocket = null;
                }
            }
        }
        /// <summary>
        /// return current IP address for server
        /// </summary>
        /// <param name="Host"></param>
        /// <returns>
        /// <para>
        /// IPAddress 
        /// </para>
        /// </returns>
        private static IPAddress GetAddress(IPHostEntry Host)
        {
            foreach (IPAddress iP in Host.AddressList)
            {
                if (iP.AddressFamily == AddressFamily.InterNetwork)//return IPv4 address only
                {
                    return iP;
                }
            }
            return IPAddress.Parse("127.0.0.1");
        }
    }
}
