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
using System.Data.SqlClient;
using System.Data.SQLite;

namespace LibProjWrite
{
    public partial class Form1 : Form
    {
        private UDPListener udpListener;
        private Thread udpListenerThread;
        private Listener listener;

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

                float entradaLimit = 1.9f;
                float saidaLimit = 3.5f;

               
                string movementType = "";
                if (zPosition < entradaLimit)
                {
                    movementType = "Entrada";
                }
                else if (zPosition > saidaLimit)
                {
                    movementType = "Saida";
                }

                
                WriteToSQLite(movementType);
            }
            else
            {
                Console.WriteLine("Insufficient joint data");
            }
        }

        private void WriteToSQLite(string movementType)
        {
         

            using (SQLiteConnection connection = new SQLiteConnection(conString))
            {
                connection.Open();

                
                string query = "INSERT INTO mobilidade (id_sensor, tipo) VALUES (@idSensor, @tipo)";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    
                    int idSensor = 1;
                    string tipo = movementType;

                    
                    if (tipo != "Entrada" && tipo != "Saida")
                    {
                        Console.WriteLine("Invalid tipo value. It must be either 'Entrada' or 'Saida'.");
                        return;
                    }

                    
                    command.Parameters.AddWithValue("@idSensor", idSensor);
                    command.Parameters.AddWithValue("@tipo", tipo);

                    
                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Data successfully written to SQLite database.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to write data to SQLite database.");
                        }
                    }
                    catch (SQLiteException ex)
                    {
                        Console.WriteLine($"SQLite Exception: {ex.Message}");
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



    

    

