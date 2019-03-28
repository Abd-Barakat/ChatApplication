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
using System.Net.Sockets;
namespace ChatApplication
{
    public partial class Main : Form
    {
        private TcpClient client = new TcpClient();
        NetworkStream network;
        public Main()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                client.Connect(IPAddress.Parse(AddressBox.Text), 5647);
                InformationPanel.Enabled = false;
                ConversationBox.Text += "Connection complete \r\n";
            }
            catch (FormatException)
            {
                if (AddressBox.Text == "")
                {
                    MessageBox.Show("Write IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Incorrect IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                AddressBox.Text = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddressBox.Text = "";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConversationBox.Text = "Client Started \r\n";
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!client.Connected)
                {
                    throw new NullReferenceException();
                }
                network = client.GetStream();
                byte[] SendData = Encoding.ASCII.GetBytes(MsgBox.Text);
                network.Write(SendData, 0, SendData.Length);
                byte[] ReceiveData = new byte[ConversationBox.Text.Length];
                int size = network.Read(ReceiveData, 0, 0);
                string Data = Encoding.ASCII.GetString(ReceiveData);
                ConversationBox.Text += Data + "\r\n";
                ConversationBox.Text += MsgBox.Text + "\r\n";
                MsgBox.Text = "";
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("You are not connected, please enter server IP first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
