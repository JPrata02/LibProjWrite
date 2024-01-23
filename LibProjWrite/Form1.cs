using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Data.Entity.Infrastructure.Design.Executor;
using cbf;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Diagnostics;
using System.Timers;

namespace LibProjWrite
{
    public partial class Form1 : Form
    {
        private UDPListener udpListener;
        private Thread udpListenerThread;
        private Listener listener;

        public Form1()
        {
            InitializeComponent();
            udpListener = new UDPListener();
            listener = new Listener();
            // Run the recovery loop indefinitely
            while (true)
            {
                // Call the Recovery function
                Recovery();

                // Introduce a delay to control the frequency of the loop (e.g., every 100 milliseconds)
                Thread.Sleep(2000);
            }


        }

        void Recovery()
        {
            listener = udpListener.GetListener();

            udpListener.StartListening();
            if (listener == null)
            {
                Console.WriteLine("There is no listener");
                return;
            }

            ConvertedBodyFrame cbf = listener.currentFrame;
            SerializableVector3[] joints = null;

            if (cbf != null)
            {
                PrintJoint2ZPosition(cbf);
            }
            else
            {
               
               Console.Write( "A leitura não se encontra disponivel");
                return;
            }

           
           
        }

        private void PrintJoint2ZPosition(ConvertedBodyFrame cbf)
        {
            SerializableVector3[] jointValues = GetJointsHeight(cbf);

           
            if (jointValues != null && jointValues.Length > 2)
            {
                float zPosition = jointValues[2].Z;
                Console.WriteLine($"Z Position of Joint[2]: {zPosition}");
            }
            else
            {
                Console.WriteLine("Insufficient joint data");
            }
        }

        private SerializableVector3[] GetJointsHeight(ConvertedBodyFrame cbf)
        {
            SerializableVector3[] jointValues = new SerializableVector3[25];

            foreach (ConvertedBody body in cbf.Bodies)
            {
                jointValues = body.Joints.Select(o => o.Position).ToArray();
            }

            return jointValues;
        }
    }
}



    

    

