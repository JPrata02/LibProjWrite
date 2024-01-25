using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using cbf;

namespace LibProjWrite
{
    public partial class Form1 : Form
    {
        private UDPListener udpListener;
        private Thread udpListenerThread;
        private Listener listener;

        private float initialZPosition = 0.0f;
        private float entradaLimit = 1.9f;  // Adjust these limits as needed
        private float saidaLimit = 4.1f;
        private bool isInEntrada = false;
        private bool isInSaida = false;

        public Form1()
        {
            InitializeComponent();
            udpListener = new UDPListener();
            listener = new Listener();

            while (true)
            {
                Recovery();
                Thread.Sleep(300);
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

            if (cbf != null)
            {
                PrintJoint2ZPosition(cbf);
            }
            else
            {
                Console.Write("A leitura não se encontra disponível");
                return;
            }
        }

        private void PrintJoint2ZPosition(ConvertedBodyFrame cbf)
        {
            SerializableVector3[] jointValues = GetJointsHeight(cbf);

            if (jointValues != null && jointValues.Length > 2)
            {
                float newZPosition = jointValues[2].Z;

                Console.WriteLine($"Z Position of Joint[2]: {newZPosition}");

                if (isInEntrada)
                {
                   
                    if (newZPosition > saidaLimit)
                    {
                        Console.WriteLine("Detected Entrada");
                        initialZPosition = 0.0f;
                        isInEntrada = false;
                        isInSaida = true;
                        return;
                    }
                }
                else if (isInSaida)
                {
                   
                    if (newZPosition < entradaLimit)
                    {
                        Console.WriteLine("Detected Saida");
                        initialZPosition = 0.0f;
                        isInEntrada = true;
                        isInSaida = false;
                        return;
                    }
                }

               
                if (initialZPosition == 0.0f)
                {
                    if (newZPosition < entradaLimit)
                    {
                        Console.WriteLine("Capturing initial Z position for Entrada");
                        initialZPosition = newZPosition;
                        isInEntrada = true;
                    }
                    else if (newZPosition > saidaLimit)
                    {
                        Console.WriteLine("Capturing initial Z position for Saida");
                        initialZPosition = newZPosition;
                        isInSaida = true;
                    }
                }
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





