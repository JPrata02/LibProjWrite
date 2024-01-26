using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
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

        private float[] initialZPositions;  
        private float entradaLimit = 1.9f; 
        private float saidaLimit = 4.1f;
        private string movementType = "";
        private bool[] isInEntrada; 
        private bool[] isInSaida;    
      

        private static string dbPath = "C:\\Users\\prata\\Desktop\\Files\\libraby.db";
        private static string conString = "Data Source=" + dbPath + ";Version=3;New=False;Compress=True";
        public Form1()
        {
            InitializeComponent();
            udpListener = new UDPListener();
            listener = new Listener();

            
            int maxBodies = 3;
            initialZPositions = new float[maxBodies];
            isInEntrada = new bool[maxBodies];
            isInSaida = new bool[maxBodies];

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
            List<SerializableVector3[]> jointValues = GetJointsHeight(cbf);

            if (jointValues != null && jointValues.Count > 0)
            {
                for (int i = 0; i < jointValues.Count; i++)
                {
                    if (jointValues[i] != null && jointValues[i].Length > 2)
                    {
                        float newZPosition = jointValues[i][2].Z;

                        Console.WriteLine($"Z Position of Joint[2] for Body {i + 1}: {newZPosition}");

                        if (isInEntrada[i])
                        {
                            if (jointValues.Any(j => j.Any(k => k.Z > saidaLimit)))
                            {
                                Console.WriteLine($"Detected Entrada for Body {i + 1}");
                                initialZPositions[i] = 0.0f;
                                isInEntrada[i] = false;
                                isInSaida[i] = true;
                                Console.WriteLine($"Transition: Entrada to Saida for Body {i + 1}");
                              
                                movementType = "Entrada";
                                //WriteDatabase(movementType);
                                Console.WriteLine("Detetou Entrada");
                                return;
                            }
                        }
                        else if (isInSaida[i])
                        {
                            if (jointValues.Any(j => j.Any(k => k.Z < entradaLimit)))
                            {
                                Console.WriteLine($"Detected Saida for Body {i + 1}");
                                initialZPositions[i] = 0.0f;
                                isInEntrada[i] = true;
                                isInSaida[i] = false;
                                Console.WriteLine($"Transition: Saida to Entrada for Body {i + 1}");
                             
                                movementType = "Saida";
                                Console.WriteLine("Detetou Saida");
                               // WriteDatabase(movementType);
                                return;
                            }
                        }

                        if (initialZPositions[i] == 0.0f)
                        {
                            if (jointValues.Any(j => j.Any(k => k.Z < entradaLimit) || j.Any(k => k.Z > saidaLimit)))
                            {
                                Console.WriteLine($"Capturing initial Z position for {(jointValues[i].Any(k => k.Z < entradaLimit) ? "Entrada" : "Saida")} for Body {i + 1}");
                                initialZPositions[i] = newZPosition;
                                isInEntrada[i] = jointValues[i].Any(k => k.Z < entradaLimit);
                                isInSaida[i] = jointValues[i].Any(k => k.Z > saidaLimit);
                                Console.WriteLine($"IsInEntrada: {isInEntrada[i]}, IsInSaida: {isInSaida[i]} for Body {i + 1}");
                            }
                        }
                    }
                }
            }
        }

        private List<SerializableVector3[]> GetJointsHeight(ConvertedBodyFrame cbf)
        {
            List<SerializableVector3[]> jointValues = new List<SerializableVector3[]>();

            foreach (ConvertedBody body in cbf.Bodies)
            {
                jointValues.Add(body.Joints.Select(o => o.Position).ToArray());
            }

            return jointValues;
        }

        private void WriteDatabase(string movementType)
        {
            try
            {
                using (SQLiteConnection con = new SQLiteConnection(conString))
                {
                    con.Open();

                    string sql = "INSERT INTO mobilidade (id_sensor, tipo) VALUES (1, @movementType)";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@movementType", movementType);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to database: {ex.Message}");
            }
        }
    }
}


