using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using cbf;

namespace LibProjWrite
{
   
    public class Listener
    {
        
        // handling variables
        private UdpClient client;
        private IPEndPoint endPoint;
        private byte[] receivedData;
        public ConvertedBodyFrame currentFrame,
            lastFrame;
        private DateTime start,
            end;

        // state variables
        private bool listening = false,
            receiving = false,
            lastFrameMatched = false;
        private string status;

        public bool Listening { get { return listening; } }
        public bool Receiving { get { return receiving; } }
        public string Status { get { return status; } }

        public Listener() { }

        /// <summary>
        /// Starts listening for UDP transmissions.
        /// </summary>
        /// <param name="ip">The IP address to receive from</param>
        /// <param name="port">The port to listen on</param>
        public void StartListening(IPAddress ip, int port, int id, float compareValue)
        {
            this.listening = true;

            // Initialize the client and other necessary members
            endPoint = new IPEndPoint(ip, port);
            client = new UdpClient();
            client.ExclusiveAddressUse = false;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(endPoint);

          //  Console.WriteLine("Setup Done");

            // Start the asynchronous receive operation
            client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }

        /// <summary>
        /// Stops listening for transmissions.
        /// </summary>
        public void StopListening()
        {
            this.listening = false;

            // Close the client to stop listening
            client?.Close();
        }

        
        


        /// <summary>
        /// Asynchronous callback for receiving transmissions.
        /// </summary>
        /// <param name="result">Operation status</param>
        public void ReceiveCallback(IAsyncResult result)
        {
            if (!listening)
                return;

            try
            {
                receivedData = client.EndReceive(result, ref endPoint);
                currentFrame = Unserialize(receivedData);
                

                // Continue listening if the process has not been stopped
                if (this.listening)
                {
                    client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReceiveCallback: {ex.Message}");
            }
        }
        /// <summary>
        /// Restores an object from the received byte array and tries to cast it to a ConvertedBodyFrame.
        /// </summary>
        /// <param name="data">The received data</param>
        /// <returns>A ConvertedBodyFrame containing 0-6 bodies and a time stamp</returns>
        private ConvertedBodyFrame Unserialize(byte[] data)
        {
            ConvertedBodyFrame cbf = null;
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    ms.Write(data, 0, data.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
                    cbf = (ConvertedBodyFrame)formatter.Deserialize(ms);
                }
                catch (SerializationException e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {

                }

                return (ConvertedBodyFrame)cbf;
            }
        }

    }
}
