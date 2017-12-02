using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class ClientManager
    {
        Socket clientSocket;
        Thread clientThread;
        string id;

        public ClientManager(Socket cSocket)
        {
            id = Guid.NewGuid().ToString();
            clientSocket = cSocket;

            clientThread = new Thread(Server.DataIn);
            clientThread.Start(clientSocket);
        }
    }
}