﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
namespace Server
{
    class Server
    {
        // Incoming data from the client.  
        static string data = null;
        static List<Socket> clients;
        static Socket listener;

        public static void StartListening()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];
 
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);


            // Create a TCP/IP socket.  

            listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            clients = new List<Socket>();
            Thread listenerConnectionThread = new Thread(ListenThreadConnection);
            listenerConnectionThread.Start();
            Thread listenerThread = new Thread(DataIn);
            listenerThread.Start();
        }

        static void ListenThreadConnection()
        {
            while (true)
            {
                while (clients.Count != 2)
                {
                    listener.Listen(20);
                    Socket client = listener.Accept();
                    clients.Add(client);
                    Console.WriteLine(clients.Count + " clients connecté");
                }

                foreach (Socket c in clients)
                {
                    byte[] strMessage = Encoding.UTF8.GetBytes("StartGame");
                    c.Send(strMessage);
                }
            }
        }

        public static void DataIn()
        {
            byte[] bytes;
            bytes = new byte[200];
            foreach (Socket c in clients)
            {
                int bytesRec = c.Receive(bytes);

                string msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine(msg);
            }

        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}
