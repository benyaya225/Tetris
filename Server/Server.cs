using System;
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
            Thread listenerThread = new Thread(DataIn);
            listenerThread.Start();
            Thread ConnectionThread = new Thread(DataOut);
            ConnectionThread.Start();



        }
        /// <summary>
        /// Send Data to the clients 
        /// </summary>
        static void DataOut()
        {
            bool gameLaunched = false;
            while (true)
            {
                //wait to have at  least 2 clients
                while (clients.Count != 2)
                {
                    listener.Listen(20);
                    Socket client = listener.Accept();
                    clients.Add(client);
                    Console.WriteLine(clients.Count + " clients connecté");
                }
                if (gameLaunched == false)
                {
                    foreach (Socket client in clients)
                    {
                        //ask to ll client to start the game
                        byte[] strMessage = Encoding.ASCII.GetBytes("StartGame");
                        client.Send(strMessage);
                        gameLaunched = true;
                    }
                    
                }
               /* while (clients.Count >= 2)
                {
                //send 1 or 2 to produce the block for the client
                    Random rand = new Random();
                    int nbBlock = rand.Next(1, 3);
                    foreach (Socket client in clients)
                    {
                        byte[] strMessage = Encoding.ASCII.GetBytes(Convert.ToString(nbBlock));
                        client.Send(strMessage);
                    }
                }*/
                
            }
        }
        /// <summary>
        ///Incoming data from the client. 
        /// </summary>
        static void DataIn()
        {
            //Wait for the connection to establish
            Thread.Sleep(5000);
            byte[] bytes;
            //if there is only one client stop listening
            while (clients.Count >1)
            {
                try
                {
                    foreach (Socket client in clients)
                    {
                        bytes = new byte[500];
                        int bytesRec = client.Receive(bytes);

                        string msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        Console.WriteLine(msg);
                        switch (msg)
                        {
                            case "Loose":
                                Console.WriteLine(client.ToString() + " Lost");
                                client.Close();
                                break;

                            case "line removed":
                                //send a penalty to all client excepte the one who removed the line
                                foreach (Socket c in clients)
                                {
                                    if (c != client)
                                    {
                                        byte[] strMessage = Encoding.ASCII.GetBytes("Penalty");
                                        c.Send(strMessage);
                                    }
                                }
                                break;
                        }
                        if (msg.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("The server has disconnected!");
                    Environment.Exit(0);
                }
            }
        }

        public static int Main(String[] args)
        {
            StartListening();
            return 0;
        }
    }
}

