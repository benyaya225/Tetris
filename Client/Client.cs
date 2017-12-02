using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        static Socket sender;
        static Game newGame = new Game();
        public static void Main(String[] args)
        {
            StartClient();
        }

        public static void StartClient()
        {
            
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.  
                sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);
                    
                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());
                    Thread listenerThread = new Thread(ListenServer);
                    listenerThread.Start();
                    Thread ligneRemovedThread = new Thread(SendRemovedLines);
                    ligneRemovedThread.Start();
                    Thread endThread = new Thread(GameOver);
                    endThread.Start();
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void ListenServer()
        {
            byte[] bytes;
            while (true)
            {
                try
                {
                    bytes = new byte[200];
                    int bytesRec = sender.Receive(bytes);

                    string msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    switch(msg)
                    {
                        case "StartGame":

                            Console.WriteLine("Debut dans 5 secondes");
                            Thread.Sleep(5000);
                            Console.Clear();
                            newGame.LaunchGame();
                            break;

                        case "EndGame":
                            Environment.Exit(0);
                            break;
                    }


                }
                catch (SocketException ex)
                {
                    Console.WriteLine("The server has disconnected!");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
        }

        static void SendRemovedLines()
        {
            while (true)
            {
                int linesRemoved = newGame.CheckForFullLines();
                if (linesRemoved  >0)
                {
                    
                    byte[] msg = Encoding.UTF8.GetBytes(linesRemoved+" line(s) removed");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);
                }
            }
        }

        static void GameOver()
        {
            bool end = false;
            while (end == false)
            {
                if (newGame.End == true)
                {
                    Console.Clear();
                    Console.WriteLine("Game Over");
                    end = true;
                }
            }
        }
    }
}
