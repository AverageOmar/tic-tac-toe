using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using Shared;

namespace SimpleChatClient
{
    public partial class SimpleChatClient : Form
    {
        int player = 0;
        static Random rnd = new Random();
        int turn = 1;
        bool winnerFound = false;
        private Object thisLock = new Object(); 

        int TL = 0;
        int TM = 0;
        int TR = 0;
        int ML = 0;
        int MM = 0;
        int MR = 0;
        int BL = 0;
        int BM = 0;
        int BR = 0;

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        
        public SimpleChatClient()
        {
            InitializeComponent();
            _tcpClient = new TcpClient();
        }
        public bool Connect(string hostname, int port)
        {
            try
            {
                _tcpClient.Connect(hostname, port);
                _stream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_stream, Encoding.UTF8);
                _reader = new BinaryReader(_stream, Encoding.UTF8);

                Thread Server = new Thread(ProcessServerResponse);
                Server.Start();

            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }

            return true;
        }

        private void ProcessServerResponse()
        {
            int noOfIncomingBytes;
            while ((noOfIncomingBytes = _reader.ReadInt32()) != 0)
            {
                byte[] bytes = _reader.ReadBytes(noOfIncomingBytes);
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream memoryStream = new MemoryStream(bytes);
                Packet packet = bf.Deserialize(memoryStream) as Packet;

                switch (packet.type)
                {
                    case PacketType.CHATMESSAGE:
                        lock (thisLock)
                        {
                            string message = ((ChatMessagePacket)packet).message;

                            richTextBox1.Invoke(new AppendTextDelgate(AppendText), new object[] { message });
                        }
                        break;
                    case PacketType.TURN:
                        string letter = ((TurnPacket)packet).letter;
                        lock (thisLock)
                        {
                            turn = ((TurnPacket)packet).turn;
                        }
                        if (((TurnPacket)packet).button == "tl")
                        {
                            btnTL.Invoke(new tlDelegate(tlChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "tm")
                        {
                            btnTM.Invoke(new tmDelegate(tmChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "tr")
                        {
                            btnTR.Invoke(new trDelegate(trChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "ml")
                        {
                            btnML.Invoke(new mlDelegate(mlChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "mm")
                        {
                            btnMM.Invoke(new mmDelegate(mmChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "mr")
                        {
                            btnMR.Invoke(new mrDelegate(mrChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "bl")
                        {
                            btnBL.Invoke(new blDelegate(blChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "bm")
                        {
                            btnBM.Invoke(new bmDelegate(bmChange), new object[] { letter });
                        }
                        if (((TurnPacket)packet).button == "br")
                        {
                            btnBR.Invoke(new brDelegate(brChange), new object[] { letter });
                        }
                        button1.Invoke(new checkWinnerDelgate(checkWinner));
                        if (player == ((TurnPacket)packet).turn)
                        {
                            if (winnerFound == false)
                            {
                                button1.Invoke(new enableButtonDelgate(enableButtons));
                            }
                        }
                        break;
                    case PacketType.PLAYER:
                        lock (thisLock)
                        {
                            player = ((PlayerPacket)packet).player;
                        }
                        if (player == 1)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are X" });
                        }
                        if (player == 2)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are O" });
                        }
                        if (player > 2)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are Spectating" });
                        }
                        if (player != turn)
                        {
                            button1.Invoke(new disableButtonDelgate(disableButtons));
                        }
                        break;
                    case PacketType.RESET:
                        btnReset.Invoke(new resetDelegate(reset));
                        if (player == 1)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are X" });
                        }
                        if (player == 2)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are O" });
                        }
                        if (player > 2)
                        {
                            lblPlayer.Invoke(new AppendLabelDelegate(changeLabel), new object[] { "You are Spectating" });
                        }
                        break;
                }               
            }
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect("127.0.0.1", 4444);
        }

        public void Send(Packet data)
        {
            MemoryStream mem = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mem, data);
            byte[] buffer = mem.GetBuffer();
            
            _writer.Write(buffer.Length);
            _writer.Write(buffer);
            _writer.Flush();
        }

        public void SendReset()
        {
            if (!_tcpClient.Connected)
                return;

            ResetPacket resetting = new ResetPacket();
            Send(resetting);
        }

        public void SendText(string text)
        {
            if (!_tcpClient.Connected)
                return;

            ChatMessagePacket chatMessagePacket = new ChatMessagePacket(text);
            Send(chatMessagePacket);
        }

        public void SendTurn(int t, string b, string l)
        {
            if (!_tcpClient.Connected)
                return;

            TurnPacket turnPacket = new TurnPacket(t, b, l);
            Send(turnPacket);
        }

        private void SetNickName(string nickname)
        {
            NickNamePacket chatMessagePacket = new NickNamePacket(nickname);
            Send(chatMessagePacket);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        delegate void AppendLabelDelegate(string text);
        public void changeLabel(string text)
        {
            lblPlayer.Text = text;
        }

        delegate void AppendTextDelgate(string text);
        public void AppendText(string text)
        {
            richTextBox1.Text += text + "\n";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (textBox2.Text == "" || textBox2.Text == " " || textBox2.Text == "Set Nickname")
            {
                textBox1.Text = "Please set a name";
            }
            else
            {
                textBox2.Enabled = false;
                string name = textBox2.Text;
                string text = textBox1.Text;
                SendText(name + ": " + text);
                textBox1.Text = "";
                _writer.Flush();
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                if (textBox2.Text == "" || textBox2.Text == " " || textBox2.Text == "Set Nickname")
                {
                    textBox1.Text = "Please set a name";
                }
                else
                {
                    textBox2.Enabled = false;
                    string name = textBox2.Text;
                    string text = textBox1.Text;
                    SendText(name + ": " + text);
                    textBox1.Text = "";
                    _writer.Flush();
                }
            }
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        delegate void checkWinnerDelgate();
        public void checkWinner()
        {
            //top horizontal
            if (TL == 1 && TM == 1 && TR == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TL == 2 && TM == 2 && TR == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //mid horizontal
            if (ML == 1 && MM == 1 && MR == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (ML == 2 && MM == 2 && MR == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //bot horizontal
            if (BL == 1 && BM == 1 && BR == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (BL == 2 && BM == 2 && BR == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //left verticle
            if (TL == 1 && ML == 1 && BL == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TL == 2 && ML == 2 && BL == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //mid verticle
            if (TM == 1 && MM == 1 && BM == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TM == 2 && MM == 2 && BM == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //right verticle
            if (TR == 1 && MR == 1 && BR == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if(TR == 2 && MR == 2 && BR == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //diagonals
            if (TL == 1 && MM == 1 && BR == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TL == 2 && MM == 2 && BR == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TR ==1 && MM == 1 && BL == 1)
            {
                lblPlayer.Text = "X wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }
            if (TR == 2 && MM == 2 && BL == 2)
            {
                lblPlayer.Text = "O wins";
                disableButtons();
                winnerFound = true;
                btnReset.Visible = true;
            }

            //draw
            if (TL > 0 && TM > 0 && TR > 0 && ML > 0 && MM > 0 && MR > 0 && BL > 0 && BM > 0 && BR > 0)
            {
                btnReset.Visible = true;
                if (winnerFound == false)
                {
                    lblPlayer.Text = "its a draw";
                }
            }
        }

        delegate void disableButtonDelgate();
        public void disableButtons()
        {
            btnTL.Enabled = false;
            btnTM.Enabled = false;
            btnTR.Enabled = false;
            btnML.Enabled = false;
            btnMM.Enabled = false;
            btnMR.Enabled = false;
            btnBL.Enabled = false;
            btnBM.Enabled = false;
            btnBR.Enabled = false;
        }

        delegate void enableButtonDelgate();
        public void enableButtons()
        {
            if (TL == 0)
            {
                btnTL.Enabled = true;
            }
            if (TM == 0)
            {
                btnTM.Enabled = true;
            }
            if (TR == 0)
            {
                btnTR.Enabled = true;
            }
            if (ML == 0)
            {
                btnML.Enabled = true;
            }
            if (MM == 0)
            {
                btnMM.Enabled = true;
            }
            if (MR == 0)
            {
                btnMR.Enabled = true;
            }
            if (BL == 0)
            {
                btnBL.Enabled = true;
            }
            if (BM == 0)
            {
                btnBM.Enabled = true;
            }
            if (BR == 0)
            {
                btnBR.Enabled = true;
            }
        }

        delegate void tlDelegate(string l);
        public void tlChange(string l)
        {
            btnTL.Text = l;
            if (l == "X")
            {
                TL = 1;
            }
            if (l == "O")
            {
                TL = 2;
            }
        }

        delegate void tmDelegate(string l);
        public void tmChange(string l)
        {
            btnTM.Text = l;
            if (l == "X")
            {
                TM = 1;
            }
            if (l == "O")
            {
                TM = 2;
            }
        }

        delegate void trDelegate(string l);
        public void trChange(string l)
        {
            btnTR.Text = l;
            if (l == "X")
            {
                TR = 1;
            }
            if (l == "O")
            {
                TR = 2;
            }
        }

        delegate void mlDelegate(string l);
        public void mlChange(string l)
        {
            btnML.Text = l;
            if (l == "X")
            {
                ML = 1;
            }
            if (l == "O")
            {
                ML = 2;
            }
        }

        delegate void mmDelegate(string l);
        public void mmChange(string l)
        {
            btnMM.Text = l;
            if (l == "X")
            {
                MM = 1;
            }
            if (l == "O")
            {
                MM = 2;
            }
        }

        delegate void mrDelegate(string l);
        public void mrChange(string l)
        {
            btnMR.Text = l;
            if (l == "X")
            {
                MR = 1;
            }
            if (l == "O")
            {
                MR = 2;
            }
        }

        delegate void blDelegate(string l);
        public void blChange(string l)
        {
            btnBL.Text = l;
            if (l == "X")
            {
                BL = 1;
            }
            if (l == "O")
            {
                BL = 2;
            }
        }

        delegate void bmDelegate(string l);
        public void bmChange(string l)
        {
            btnBM.Text = l;
            if (l == "X")
            {
                BM = 1;
            }
            if (l == "O")
            {
                BM = 2;
            }
        }

        delegate void brDelegate(string l);
        public void brChange(string l)
        {
            btnBR.Text = l;
            if (l == "X")
            {
                BR = 1;
            }
            if (l == "O")
            {
                BR = 2;
            }
        }

        private void btnTL_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }
            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnTL.Text = "X";
                    TL = 1;
                    SendTurn(turn, "tl", "X");
                }
                if (player == 2)
                {
                    btnTL.Text = "O";
                    TL = 2;
                    SendTurn(turn, "tl", "O");
                }
                checkWinner();
            }
        }

        private void btnTM_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnTM.Text = "X";
                    TM = 1;
                    SendTurn(turn, "tm", "X");
                }
                if (player == 2)
                {
                    btnTM.Text = "O";
                    TM = 2;
                    SendTurn(turn, "tm", "O");
                }
                checkWinner();
            }
        }

        private void btnTR_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnTR.Text = "X";
                    TR = 1;
                    SendTurn(turn, "tr", "X");
                }
                if (player == 2)
                {
                    btnTR.Text = "O";
                    TR = 2;
                    SendTurn(turn, "tr", "O");
                }
                checkWinner();
            }
        }

        private void btnML_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnML.Text = "X";
                    ML = 1;
                    SendTurn(turn, "ml", "X");
                }
                if (player == 2)
                {
                    btnML.Text = "O";
                    ML = 2;
                    SendTurn(turn, "ml", "O");
                }
                checkWinner();
            }
        }

        private void btnMM_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnMM.Text = "X";
                    MM = 1;
                    SendTurn(turn, "mm", "X");
                }
                if (player == 2)
                {
                    btnMM.Text = "O";
                    MM = 2;
                    SendTurn(turn, "mm", "O");
                }
                checkWinner();
            }
        }

        private void btnMR_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnMR.Text = "X";
                    MR = 1;
                    SendTurn(turn, "mr", "X");
                }
                if (player == 2)
                {
                    btnMR.Text = "O";
                    MR = 2;
                    SendTurn(turn, "mr", "O");
                }
                checkWinner();
            }
        }

        private void btnBL_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnBL.Text = "X";
                    BL = 1;
                    SendTurn(turn, "bl", "X");
                }
                if (player == 2)
                {
                    btnBL.Text = "O";
                    BL = 2;
                    SendTurn(turn, "bl", "O");
                }
                checkWinner();
            }
        }

        private void btnBM_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnBM.Text = "X";
                    BM = 1;
                    SendTurn(turn, "bm", "X");
                }
                if (player == 2)
                {
                    btnBM.Text = "O";
                    BM = 2;
                    SendTurn(turn, "bm", "O");
                }
                checkWinner();
            }
        }

        private void btnBR_Click(object sender, EventArgs e)
        {
            disableButtons();
            if (player == 1)
            {
                turn = 2;
            }
            if (player == 2)
            {
                turn = 1;
            }

            if (winnerFound == false)
            {
                if (player == 1)
                {
                    btnBR.Text = "X";
                    BR = 1;
                    SendTurn(turn, "br", "X");
                }
                if (player == 2)
                {
                    btnBR.Text = "O";
                    BR = 2;
                    SendTurn(turn, "br", "O");
                }
                checkWinner();
            }
        }

        delegate void resetDelegate();
        public void reset()
        {
            btnReset.Visible = false;

            winnerFound = false;
            turn = rnd.Next(1, 3);

            TL = 0;
            TM = 0;
            TR = 0;
            ML = 0;
            MM = 0;
            MR = 0;
            BL = 0;
            BM = 0;
            BR = 0;

            btnTL.Text = " ";
            btnTM.Text = " ";
            btnTR.Text = " ";
            btnML.Text = " ";
            btnMM.Text = " ";
            btnMR.Text = " ";
            btnBL.Text = " ";
            btnBM.Text = " ";
            btnBR.Text = " ";
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SendReset();
            SendTurn(turn, null, null);
        }
    }
}
