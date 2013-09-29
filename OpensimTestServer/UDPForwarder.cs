using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Web;
using System.Net.Sockets;
using System.Threading;

namespace OpensimTestServer
{
    class UDPForwarder
    {
        //private static byte[] ack;

        private String fwdIp;
        private int fwdPort;
        private int boundPort;

        private IPEndPoint server;
        private IPEndPoint viewer;

        public bool Listening;

        public UDPForwarder(String fwdIp, int fwdPort)
        {
            this.fwdIp = fwdIp;
            this.fwdPort = fwdPort;
            this.boundPort = fwdPort;
            Listening = false;

            server = new IPEndPoint(IPAddress.Parse(fwdIp), fwdPort);
            viewer = null;
        }

        public UDPForwarder(String fwdIp, int fwdPort, int boundPort) : this(fwdIp, fwdPort)
        {
            this.boundPort = boundPort;
        }

        public void StartListening()
        {
            Console.WriteLine("UDPForwarder now accepting packets.");
            Listening = true;
            new Thread(Listen).Start();
        }

        private void Listen()
        {
            UdpClient udpClient = new UdpClient(boundPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, fwdPort); //Any is NOT safe... change it so it's the loop back and the server ip
 
            byte[] receive_byte_array;

            try
            {
                while (Listening)
                {
                    receive_byte_array = udpClient.Receive(ref groupEP);
                    Console.WriteLine("Packet Received!");
                    
                    ThreadPool.QueueUserWorkItem((ThreadContext) => handlePacket(receive_byte_array, groupEP, udpClient)) ;
                }
                   
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            udpClient.Close();  
        }

        private void handlePacket(byte[] receive_byte_array, IPEndPoint groupEP, UdpClient udpClient)
        {
            //Debug
            //Console.WriteLine("Received a broadcast from {0}", groupEP.ToString() );
            //String received_data = Encoding.ASCII.GetString(receive_byte_array, 0, receive_byte_array.Length);
            //Console.WriteLine(received_data);

            //First packet received is from the client, this is always guarenteed.
            if (viewer == null)
            {
                viewer = new IPEndPoint(IPAddress.Loopback, groupEP.Port);
            }

            //If received packet is from the viewer, send the packet to the server
            if (groupEP.Address.Equals(viewer.Address))
            {
                //Console.WriteLine("Forwarding Packet to server...");
                udpClient.Client.SendTo(receive_byte_array, server);
            }
            else //send to the server
            {
                //if (ack == null)
                //{
                //    ack = receive_byte_array;
                //}
                //Console.WriteLine("Forwarding Packet to viewer...");
                udpClient.Client.SendTo(receive_byte_array, viewer);
            }
        }
    }
}
