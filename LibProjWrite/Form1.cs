using System;
using System.Data.SQLite;
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
        private float[] zPositionBuffer = new float[5];
        private int bufferIndex = 0;

        private static string dbPath = "C:\\Users\\prata\\Desktop\\Files\\libraby.db";
        private static string conString = "Data Source=" + dbPath + ";Version=3;New=False;Compress=True";

        public Form1()
        {
            InitializeComponent();
            udpListener = new UDPListener();
            listener = new Listener();

            while (true)
            {
                Recovery();
                Thread.Sleep(500);
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

                float entradaLimit = 2.0f;
                float saidaLimit = 4.0f;
                float zDifferenceThreshold = 0.5f;

                if (initialZPosition == 0.0f)
                {
                    if (newZPosition < entradaLimit)
                    {
                        Console.WriteLine("Capturing initial Z position");
                        initialZPosition = newZPosition;
                        Console.WriteLine($"Initial Z Position: {initialZPosition}");
                    }
                }
                else
                {
                    zPositionBuffer[bufferIndex] = newZPosition;
                    bufferIndex = (bufferIndex + 1) % zPositionBuffer.Length;

                    float movingAverage = zPositionBuffer.Average();

                    Console.WriteLine($"Moving Average: {movingAverage}");

                    if (newZPosition < entradaLimit && movingAverage < entradaLimit)
                    {
                        if (initialZPosition - newZPosition > zDifferenceThreshold)
                        {
                            Console.WriteLine("Detected Saida");
                            initialZPosition = 0.0f;
                        }
                    }
                    else if (newZPosition > saidaLimit && movingAverage > saidaLimit)
                    {
                        if (newZPosition - initialZPosition > zDifferenceThreshold)
                        {
                            Console.WriteLine("Detected Entrada");
                            initialZPosition = 0.0f;
                        }
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





