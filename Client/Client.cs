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
                    Thread sendThread = new Thread(SendToServer);
                    sendThread.Start();
                    Thread listenerThread = new Thread(ListenServer);
                    listenerThread.Start();


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
        /// <summary>
        /// Listen for the incoming message from the server
        /// </summary>
        static void ListenServer()
        {
            byte[] bytes;
            while (sender.Connected)
            {
                try
                {
                    
                    bytes = new byte[500];
                    int bytesRec = sender.Receive(bytes);

                    string msg = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    switch (msg)
                    {
                        //i the server ask to start the game
                        case "StartGame":

                            Console.WriteLine("Game begin in 5 seconds");
                            Thread.Sleep(5000);
                            Console.Clear();
                            newGame.LaunchGame();
                            break;
                        case "You Win":
                            Console.Clear();
                            Console.WriteLine(msg);
                            break;
                            //when the server send "1" a block of one square should be displayed in the game
                        case "1":
                            //when he send "2" a block of four square
                            break;
                        case "2":

                            break;
                            //add a penalty
                        case "Penalty":
                            newGame.Penalty += 1;
                            break;
                    }


                }
                catch (SocketException ex)
                {
                    Console.Clear();
                    Console.WriteLine("The server has disconnected!");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
            }
        }
        /// <summary>
        /// Function to Send message to the server
        /// </summary>
        static void SendToServer()
        {
            bool end = false;
            while (end == false)
            {
                //check for removed lines
                if (newGame.RemovedLines > 0)
                {

                    byte[] msg = Encoding.UTF8.GetBytes("line removed");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);
                }

                    if (newGame.End == true)
                {
                    //send loose to the server in the case of a defeat
                    end = true;
                    Console.Clear();
                    Console.WriteLine("Game Over");
                    byte[] msgSent = Encoding.ASCII.GetBytes("Loose<EOF>");
                    Console.ReadKey();
                    sender.Close();
                }
            }
        }
    }
}
