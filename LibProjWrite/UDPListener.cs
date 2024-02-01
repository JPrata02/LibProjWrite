using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;


namespace LibProjWrite
{
    public class UDPListener 
    {
        private Listener listener;

        private Stopwatch stopwatch = new Stopwatch();
        private bool validIp = false,
                    validPort = true,
                    validFloat = false;

        private IPAddress ipAddress = IPAddress.Any;

        private int port = 1234,
                    jointID;

        private float compareValue,
                        InstantiationTimer = 0.1f;

        private string ipErrorMessage = "",
                        portErrorMessage = "",
                        heightErrorMessage = "";

     

        void Start()
        {
            ipAddress = IPAddress.Parse("127.0.0.1");
            listener = new Listener();
            StartListening();
            Console.WriteLine(ipErrorMessage);
            Console.WriteLine(portErrorMessage);
            Console.WriteLine(heightErrorMessage);

        }

        public UDPListener()
        {
            listener = new Listener(); // Initialize the listener instance
                                       // Other initialization code if needed
        }
        public Listener GetListener()
        {
            return listener;
        }

        void Update()
        {
            UpdateStatus();
        }

        public void StartListening()
        {
            // toggle the "listening" state and start or stop the logger
            if (listener.Listening)
            {
              //  Console.WriteLine("Was something ehre");
                listener.StopListening();
                UpdateStatus();

            }
            else
            {
              //  Console.WriteLine("Start New Listening");
                listener.StartListening(ipAddress, port, 0, compareValue);
                UpdateStatus();

            }
        }

        private void UpdateStatus()
        {
            // Start the stopwatch if it's not already running
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
            }

            // Calculate delta time using the elapsed time in seconds
            float deltaTime = (float)stopwatch.Elapsed.TotalSeconds;

            // Decrement the timer
            InstantiationTimer -= deltaTime;

            if (InstantiationTimer <= 0)
            {
                // update the listen button
                if (listener.Listening)
                {
                    // Do something when listening
                   // Console.WriteLine("Listening...");
                }
                else
                {
                    if (validPort)
                    {
                        // Do something when not listening but valid port
                        listener.ReceiveCallback(null);
                     //   Console.WriteLine("Start Listening");
                    }
                    else
                    {
                        // Do something when not listening and invalid port
                        Console.WriteLine("Invalid Port");
                    }
                }

                // Reset the timer
                InstantiationTimer = 0.2f;

                // Restart the stopwatch for the next cycle
                stopwatch.Restart();
            }

            // update the status label
        }

        public void StopListening()
        {
            Console.WriteLine("Stop Listening UDp");
            listener.StopListening();
            UpdateStatus();
        }

        /// <summary>
        /// Checks for validity of an entered IP address (string).
        /// </summary>
        /// <param name="ipCandidate">The string to be parsed</param>
        /// <param name="errorMessage">An error message to be displayed if the string does not validate</param>
        /// <returns>True for valid IP addresses, false for invalid ones</returns>
      

       

      
    }
}
