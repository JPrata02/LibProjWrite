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
        private bool ValidIp(string ipCandidate, out string errorMessage)
        {
            if (String.IsNullOrEmpty(ipCandidate))
            {
                ipAddress = IPAddress.Any;
                errorMessage = "";
                return true;
            }
            else
            {
                try
                {
                    ipAddress = IPAddress.Parse(ipCandidate);
                    errorMessage = "";
                    return true;
                }
                catch (ArgumentNullException)
                {
                    errorMessage = "IP address may not be null. Please enter a valid address.";
                    return false;
                }
                catch (FormatException)
                {
                    errorMessage = "IP address has invalid format. Please try again.";
                    return false;
                }
                catch (Exception)
                {
                    errorMessage = "An unhandled exception has occurred. Please report this!";
                    return false;
                }
            }
        }

        /// <summary>
        /// Checks for validity of an entered port number (string).
        /// </summary>
        /// <param name="portCandidate">The string to be parsed</param>
        /// <param name="errorMessage">An error message to be displayed if the string does not validate</param>
        /// <returns>True for valid port numbers, false for invalid ones</returns>
        private bool ValidPort(string portCandidate, out string errorMessage)
        {
            if (Int32.TryParse(portCandidate, out port))
            {
                if (port >= 1 && port <= 65535)
                {
                    errorMessage = "";
                    return true;
                }
                else
                {
                    errorMessage = "Port out of bounds. Please enter a port between 1 and 65535";
                    return false;
                }
            }
            else
            {
                errorMessage = "Port invalid. Please enter a valid integer.";
                return false;
            }
        }

        /// <summary>
        /// Checks for validity of an entered height parameter (string).    
        /// </summary>
        /// <param name="heightCandidate">The string to be parsed</param>
        /// <param name="errorMessage">An error message to be displayed if the string does not validate</param>
        /// <returns>True for valid float point numbers, false for invalid ones</returns>
        private bool ValidFloat(string heightCandidate, out string errorMessage)
        {
            if (float.TryParse(heightCandidate, out compareValue))
            {
                errorMessage = "";
                return true;
            }
            else
            {
                errorMessage = "Cannot parse the input. Please enter a valid number in the format 0.12345";
                return false;
            }
        }
    }
}
