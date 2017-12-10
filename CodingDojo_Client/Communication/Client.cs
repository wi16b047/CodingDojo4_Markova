using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CodingDojo_Client.Communication
{
    class Client
    {
        byte[] buffer = new byte[512]; //(binäre) Folge aus 8 Bit
        Socket clientsocket;
        //Socket: Abstraktion, die einer Anwendung ermöglicht, sich in ein bestehendes Netzwerk einzuklinken um mit anderen Applikationen zu kommunizieren
        Action<string> MessageInformer;
        Action AbortInformer;

        public Client(string ip, int port, Action<string> messageInformer, Action abortInformer)
        {
            try
            {
                this.AbortInformer = abortInformer;
                this.MessageInformer = messageInformer;
                TcpClient client = new TcpClient();
                client.Connect(System.Net.IPAddress.Parse(ip), port);
                clientsocket = client.Client;
                StartReceiving();

            }
            catch (Exception)
            {
                //Methode vom delegate aufgerufen
                messageInformer("Server not ready!");
                AbortInformer(); //reset Client Communication
            }

        }

        internal void Send(object p)
        {
            throw new NotImplementedException();
        }

        private void StartReceiving()
        {
            //Start receiving in new Thread
            Task.Factory.StartNew(Receive);
        }

        private void Receive()
        {
            string message = "";
            while (!message.Contains("@quit"))
            {
                int length = clientsocket.Receive(buffer);
                message = Encoding.UTF8.GetString(buffer, 0, length);
                //inform GUI via delegate
                MessageInformer(message);
            }
            Close();
        }

        public void Send(string message)
        {
            if (clientsocket != null) //check if clientsocket connected!
            {
                clientsocket.Send(Encoding.UTF8.GetBytes(message));
            }
        }
        private void Close()
        {
            clientsocket.Close();
            AbortInformer();
        }
    }
}
