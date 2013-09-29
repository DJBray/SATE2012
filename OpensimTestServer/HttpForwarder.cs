using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using OpenMetaverse;
using System.Net;
using System.Web;

namespace OpensimTestServer
{
    class HttpForwarder
    {
        private const int listenPort = 8002;
        private UDPForwarder simulator;
        private UDPForwarder devotion;
        private bool loggedIn;
        private bool devotionUp;
        private byte[] serverResponse;
        private bool listening;

        private String udpIp;
        private int udpPort;

        private const int localUdpPort = 9001;

        public HttpForwarder() 
        {
            listening = false;
            loggedIn = false;
            devotionUp = false;
            simulator = null;
        }

        public void StartListening()
        {
            new Thread(startListening).Start();
        }

        private void startListening()
        {
            if (!listening)
            {
                listening = true;

                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:" + listenPort + "/");
                listener.Start();

                while (listening)
                {
                    HttpListenerContext context = listener.GetContext();
                    if (!loggedIn || devotionUp)
                        ThreadPool.QueueUserWorkItem((ThreadContext) => ForwardMessageToServer(context));
                    else
                        ThreadPool.QueueUserWorkItem((ThreadContext) => ConnectDevotion(context));
                }
                listener.Stop();
            }
        }

        public void StopListening()
        {
            object threadLock = new object();
            lock (threadLock)
            {
                if (listening)
                    listening = false;
            }
        }

        private void ForwardMessageToServer(HttpListenerContext context)
        {
            HttpWebRequest newRequest = (HttpWebRequest)HttpWebRequest.Create("http://virtualdiscoverycenter.net:8002/");

            //Listen for a request on the virtual server
            Console.WriteLine("Recieved message!");


            //Forward request to the OpenSim Server
            Console.WriteLine("Constructing forwarded request...");
            newRequest.ContentType = context.Request.ContentType;
            newRequest.Method = context.Request.HttpMethod;
            newRequest.UserAgent = context.Request.UserAgent;

            Console.WriteLine("\tReading in data stream...");
            byte[] data = ReadIntoBytes(context.Request.InputStream);
            newRequest.GetRequestStream().Write(data, 0, data.Length);
            newRequest.GetRequestStream().Close();
            Console.WriteLine("Sending forwarded packet to http://virtualdiscoverycenter.net:8002/");


            //Get the response from the OpenSim server and forward it to the viewer
            Console.WriteLine("Getting server's response...");
            WebResponse response = newRequest.GetResponse();
            context.Response.ContentType = response.ContentType;

            byte[] responseData = ReadIntoBytes(response.GetResponseStream());

            SniffAndModify(ref responseData, out udpIp, out udpPort, "127.0.0.1", localUdpPort);
            serverResponse = responseData;
            simulator = new UDPForwarder(udpIp, udpPort);
            simulator.StartListening();

            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
            context.Response.OutputStream.Close();
            Console.WriteLine("Forwarding server's response to the viewer.");
            context.Response.Close();

            loggedIn = true;
        }

        private void ConnectDevotion(HttpListenerContext context)
        {
            SniffAndModify(ref serverResponse, out udpIp, out udpPort, "127.0.0.1", localUdpPort);

            devotion = new UDPForwarder(udpIp, udpPort, udpPort+1);
            devotion.StartListening();

            context.Response.OutputStream.Write(serverResponse, 0, serverResponse.Length);
            context.Response.OutputStream.Close();
            Console.WriteLine("Forwarding server's response to Devotion");
            context.Response.Close();

            devotionUp = true;
        }

        private void SniffAndModify(ref byte[] data, out String udpIp, out int udpPort, String localIp, int localPort)
        {
            //TODO: Modify this method so it doesn't need to convert to a string first.
            String ipTag = "<string>";
            String portTag = "<i4>";

            //Find and replace IPAddress
            String dataInTxt = Encoding.ASCII.GetString(data);
            int index = dataInTxt.IndexOf("sim_ip");
            int start = dataInTxt.IndexOf(ipTag, index);
            int finish = dataInTxt.IndexOf("</string>", index);

            udpIp = dataInTxt.Substring(start + ipTag.Length, finish - (start + ipTag.Length));
            
            dataInTxt = dataInTxt.Remove(start + ipTag.Length, finish - (start + ipTag.Length));
            dataInTxt = dataInTxt.Insert(start + ipTag.Length, localIp);

            //Find and record port
            index = dataInTxt.IndexOf("sim_port");
            start = dataInTxt.IndexOf(portTag, index);
            finish = dataInTxt.IndexOf("</i4>", index);
            
            udpPort = int.Parse(dataInTxt.Substring(start + portTag.Length, finish - (start + portTag.Length)));

            dataInTxt = dataInTxt.Remove(start + portTag.Length, finish - (start + portTag.Length));
            dataInTxt = dataInTxt.Insert(start + portTag.Length, localPort+"");

            //Debug
            //Console.WriteLine(dataInTxt);

            data = Encoding.ASCII.GetBytes(dataInTxt);
        }

        public static byte[] ReadIntoBytes(Stream stream)
        {
            int initialLength = 8192;
            
            byte[] buffer = new byte[initialLength];
            int read = 0;

            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;

                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();

                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }

                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);

            return ret;
        }
    }
}
