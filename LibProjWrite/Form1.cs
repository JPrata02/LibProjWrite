using cbf;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibProjWrite
{
    public partial class Form1 : Form
    {
        private UDPListener udpListener;
        private Thread udpListenerThread;
        private Listener listener;

        private Dictionary<int, BodyState> bodyStates = new Dictionary<int, BodyState>();
        private float entradaLimit = 2.45f;
        private float saidaLimit = 4.0f;
        private bool isProgramRunning = false;
        private string movementType = "";

        private static string dbPath = "C:\\Users\\prata\\Desktop\\Files\\libraby.db";
        private static string conString = "Data Source=" + dbPath + ";Version=3;New=False;Compress=True";


        public Form1()
        {
            InitializeComponent();
            udpListener = new UDPListener();
            listener = new Listener();
            this.Text = "Monitoramento de Mobilidades";
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
                GetBodyID(cbf);
            }
            else
            {
                Console.Write("A leitura não se encontra disponível");
                return;
            }
        }

        private void PrintJoint2ZPosition(ConvertedBodyFrame cbf)
        {
            List<(int BodyId, SerializableVector3[] JointPositions)> bodyIdsAndJoints = GetBodyID(cbf);

            foreach (var (bodyId, jointPositions) in bodyIdsAndJoints)
            {
                if (jointPositions.Length >= 3)
                {
                    float newZPosition = jointPositions[2].Z;
                    float newXPosition = jointPositions[2].X;

                    if (IsPositionWithinLimits(newXPosition, newZPosition))
                    {
                        Console.WriteLine($"Z Position of Joint[2] for Body {bodyId}: {newZPosition}");
                        Console.WriteLine($"X Position of Joint[2] for Body {bodyId}: {newXPosition}");

                        if (!bodyStates.ContainsKey(bodyId))
                        {
                            bodyStates[bodyId] = new BodyState();
                        }

                        BodyState state = bodyStates[bodyId];

                        if (state.isInEntrada)
                        {
                            if (newZPosition > saidaLimit)
                            {
                                Console.WriteLine($"Detected Entrada for Body {bodyId}");
                                movementType = ("Entrada");
                                WriteDatabase(movementType);
                                ResetState(bodyId);
                                SetInSaidaState(bodyId);
                            }
                        }
                        else if (state.isInSaida)
                        {
                            if (newZPosition < entradaLimit)
                            {
                                Console.WriteLine($"Detected Saida for Body {bodyId}");
                                movementType = ("Saida");
                                WriteDatabase(movementType);
                                ResetState(bodyId);
                                SetInEntradaState(bodyId);
                            }
                        }

                        if (state.initialZPosition == 0.0f)
                        {
                            if (newZPosition < entradaLimit)
                            {
                                Console.WriteLine($"Capturing initial Z position for Entrada for Body {bodyId}");
                                state.initialZPosition = newZPosition;
                                SetInEntradaState(bodyId);
                            }
                            else if (newZPosition > saidaLimit)
                            {
                                Console.WriteLine($"Capturing initial Z position for Saida for Body {bodyId}");
                                state.initialZPosition = newZPosition;
                                SetInSaidaState(bodyId);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Data outside the specified range for Body {bodyId}");
                    }
                }
            }
        }

        private bool IsPositionWithinLimits(float xPosition, float zPosition)
        {
            float minXLimit = -0.5f;
            float maxXLimit = 1.5f;

            float minZLimit = 1.7f;
            float maxZLimit = 4.4f;

            return (xPosition >= minXLimit && xPosition <= maxXLimit && zPosition >= minZLimit && zPosition <= maxZLimit);
        }

        private void SetInEntradaState(int bodyId)
        {
            BodyState state = bodyStates[bodyId];
            state.isInEntrada = true;
            state.isInSaida = false;
        }

        private List<(int BodyId, SerializableVector3[] JointPositions)> GetBodyID(ConvertedBodyFrame cbf)
        {
            List<(int BodyId, SerializableVector3[] JointPositions)> bodyValues = new List<(int, SerializableVector3[])>();

            for (int i = 0; i < cbf.Bodies.Count; i++)
            {
                ConvertedBody body = cbf.Bodies[i];

                if (body != null && body.Joints.Count >= 3)
                {
                    int bodyId = i;

                    SerializableVector3[] jointPositions = body.Joints
                        .Select(o => o.Position)
                        .ToArray();

                    bodyValues.Add((bodyId, jointPositions));
                }
            }

            return bodyValues;
        }

        private void SetInSaidaState(int bodyId)
        {
            BodyState state = bodyStates[bodyId];
            state.isInEntrada = false;
            state.isInSaida = true;
        }

        private void ResetState(int bodyId)
        {
            BodyState state = bodyStates[bodyId];
            state.isInEntrada = false;
            state.isInSaida = false;
            state.initialZPosition = 0.0f;
        }

        private class BodyState
        {
            public bool isInEntrada { get; set; }
            public bool isInSaida { get; set; }
            public float initialZPosition { get; set; }
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
        private void button1_Click(object sender, EventArgs e)
        {
            if (!isProgramRunning)
            {
               
                    isProgramRunning = true;
                    Task.Run(() => StartProgram());
                    label2.Text = "Programa Ligado";
                    label2.ForeColor = System.Drawing.Color.Green;
                
            }
            else
            {
              
                isProgramRunning = false;
                label2.Text = "Programa Desligado";
                label2.ForeColor = System.Drawing.Color.Red;
            }
        }

        private void StartProgram()
        {
            Console.WriteLine("Program started!");

            while (isProgramRunning)
            {
                Recovery();
                Thread.Sleep(200);
            }

            
            Console.WriteLine("Program stopped!");
        }
    }
}