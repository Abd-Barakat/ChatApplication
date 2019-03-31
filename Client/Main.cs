using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Net.Sockets;
using System.IO;
namespace ChatApplication
{
    public partial class Main : Form
    {
        private TcpClient client = new TcpClient();
        NetworkStream network;
        Thread CtThread;
        public Main()
        {
            InitializeComponent();
            CtThread = new Thread(Listening);
            //Process[] processes = Process.GetProcessesByName("Server");
            //if (processes.Length == 0)
            //{
            //    string path = System.IO.Directory.GetParent(@"../../../").FullName;
            //    path += @"\Server\bin\Debug\Server.exe";
            //    Process.Start(path);
            //}
        }
        private void Listening()
        {
            while (true)
            {
                try
                {
                    network = client.GetStream();
                    byte[] ReceiveData = new byte[1024 * 20];
                    int size = network.Read(ReceiveData, 0, ReceiveData.Length);
                    if (size < ReceiveData.Length)
                    {
                        Array.Resize(ref ReceiveData, size);
                    }
                    string Data = Encoding.ASCII.GetString(ReceiveData);
                    Data.Replace('$', ':');
                    //ConversationBox.AppendText(Data + "\r\n");
                    SetText(Data);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private delegate void SetTextCallBack(string msg);

        private void SetText (string Msg)
        {
            if (this.ConversationBox.InvokeRequired)
            {
                SetTextCallBack setText = new SetTextCallBack(SetText);
                Invoke(setText,Msg);
            }
            else
            {
                ConversationBox.AppendText(Msg + "\r\n");
            }
        }
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NameBox.Text))
                {
                    throw new NullReferenceException();
                }
                client.Connect(IPAddress.Parse(AddressBox.Text), 5647);
                CtThread.Start();
                var buffer = new byte[1024];
                int size = client.GetStream().Read(buffer, 0, buffer.Length);
                if (size < buffer.Length)
                {
                    Array.Resize(ref buffer, size);
                }
                string ACK = Encoding.ASCII.GetString(buffer);
                InformationPanel.Enabled = false;
                //ConversationBox.Text += ACK + "\r\n";
                SetText(ACK);
            }
            catch (NullReferenceException ex)
            {
                MessageBox.Show("Please write user name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors("Please write user name", ex);
            }
            catch (FormatException)
            {
                if (AddressBox.Text == "")
                {
                    MessageBox.Show("Write IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors("Write IP Address");
                }
                else
                {
                    MessageBox.Show("Incorrect IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors("Incorrect IP Address");
                }
                AddressBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors(ex);
                AddressBox.Text = "";
            }
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!client.Connected)
                {
                    throw new NullReferenceException();
                }
                if (MsgBox.Text != "")
                {
                    MsgBox.Text = string.Concat(NameBox.Text + "$ : ", MsgBox.Text);
                    network = client.GetStream();
                    byte[] SendData = Encoding.ASCII.GetBytes(MsgBox.Text);
                    var Header = BitConverter.GetBytes(SendData.Length);
                    network.Write(Header, 0, 4);
                    network.Write(SendData, 0, SendData.Length);
                    MsgBox.Text = "";
                    MsgBox.Focus();
                }
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("You are not connected, please enter user name and server IP ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors("You are not connected, please enter user name and server IP");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors(ex);
            }
        }

        private void MsgBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                SendButton_Click(null, null);
            }
        }

        private void AddressBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                ConnectButton_Click(null, null);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.Connected)
            {
                byte[] CloseCode = new byte[1];
                network.Write(CloseCode, 0, 1);
            }
        }
        private void PrintErrors (String Message )
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt"))
            {
                stream.WriteLine("Date : " + DateTime.Now.ToLocalTime());
                stream.WriteLine("Message :");
                stream.WriteLine(Message);
                stream.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
        }
        private void PrintErrors(Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt"))
            {
                stream.WriteLine("Date : " + DateTime.Now.ToLocalTime());
                stream.WriteLine("Stack trace :");
                stream.WriteLine(ex.StackTrace);
                stream.WriteLine("Message :");
                stream.WriteLine(ex.Message);
                stream.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
        }
        private void PrintErrors(string Message,Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt"))
            {
                stream.WriteLine("Date : " + DateTime.Now.ToLocalTime());
                stream.WriteLine("Stack trace :");
                stream.WriteLine(ex.StackTrace);
                stream.WriteLine("Message :");
                stream.WriteLine(Message);
                stream.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
        }
    }
}
