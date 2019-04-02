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
        private CancellationTokenSource cancellation;
        private TcpClient client = new TcpClient();// client socket
        private NetworkStream network;// Client network stream
        private Thread CtThread;//Client thread to keep listening to server while connection open
        /// <summary>
        /// Initialize clients & create thread to listen from server
        /// </summary>
        public Main()
        {
            InitializeComponent();
            CtThread = new Thread(Listening);//create thread to listen
            CtThread.IsBackground = true;
        }
        /// <summary>
        /// listen to server while connection is opened.
        /// </summary>
        private void Listening(object cancel)
        {
            this.cancellation = (CancellationTokenSource)cancel;
            while (!cancellation.IsCancellationRequested)
            {
                try
                {
                    network = client.GetStream();//hold client stream to read and write operations.
                    byte[] ReceiveData = new byte[1024 * 20];//hold retrived data from server in bytes
                    int size = network.Read(ReceiveData, 0, ReceiveData.Length);//wait until server send some data
                    if (size == 0)//when server close the connection after connection is closed by CLIENT
                    {
                        client.Close();
                        cancellation.Cancel();
                        continue;
                    }
                    if (BitConverter.ToInt32(ReceiveData, 0) == 0)
                    {
                        throw new FullException();
                    }
                    if (size < ReceiveData.Length)//if retrived data size is less than located memory then resize memory
                    {
                        Array.Resize(ref ReceiveData, size);
                    }
                    string Data = Encoding.UTF8.GetString(ReceiveData);//transform bytes to string
                    SetText(Data);//write data to conversationbox textbox 
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Server has shut down", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors("Server has shut down", ex);
                    client = new TcpClient();//because current Socket is closed by server so new Socket need to defined
                    CtThread = new Thread(Listening);//because current thread will be terminated (deleted) so we need to define a new thread 
                    EnableInformationPanel(true);
                    cancellation.Cancel();//cancel looping
                    continue;
                }
                catch (FullException ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors(ex);
                    cancellation.Cancel();
                    continue;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors(ex);
                    cancellation.Cancel();
                    continue;
                }
            }
            cancellation.Dispose();
        }

        private delegate void EnablePanel(bool Enable);
        private delegate void SetTextCallBack(string msg);//handle SetText method to call it in the  UI thread
        private void EnableInformationPanel(bool Enable)
        {
            if (InformationPanel.InvokeRequired)
            {
                EnablePanel panel = new EnablePanel(EnableInformationPanel);
                Invoke(panel, Enable);
                cancellation = new CancellationTokenSource();
            }
            else
            {
                InformationPanel.Enabled = Enable;
            }
        }
        /// <summary>
        /// Write string to conversationBox textbox, this method can be used by different threads to write on conversationBox textbox
        /// </summary>
        /// <param name="Msg">
        /// string to write in conversationBox textbox
        /// </param>
        private void SetText(string Msg)
        {
            if (this.ConversationBox.InvokeRequired)//check if caller Id is different from creator id 
            {
                SetTextCallBack setText = new SetTextCallBack(SetText);
                Invoke(setText, Msg);//call delegate from creator thread
            }
            else//if method called from creator thread
            {
                ConversationBox.AppendText(Msg + "\r\n");
            }
        }
        /// <summary>
        /// create connection between client and server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NameBox.Text))//check if name is inserted or not
                {
                    throw new NullReferenceException();//throw exception
                }
                cancellation = new CancellationTokenSource();
                client.Connect(IPAddress.Parse(AddressBox.Text), 5647);//transform string to IP if posible or throw format exception
                CtThread.Start(cancellation);//start listen thread after connection established
                EnableInformationPanel(false);//enable information panel after connection established
            }
            catch (NullReferenceException ex)//catch exception that fired when Namebox is empty
            {
                MessageBox.Show("Please write user name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors("Please write user name", ex);
            }
            catch (FormatException ex)//catch exception that fired when AddressBox is either empty or contain invalid text
            {

                if (AddressBox.Text == "")//check if AddressBox is empty 
                {
                    MessageBox.Show("Please write IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors("Please write IP Address", ex);
                }
                else//check if AddressBox  contain invalid text than can't be converted into IP
                {
                    MessageBox.Show("Incorrect IP Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    PrintErrors("Incorrect IP Address", ex);
                }
                AddressBox.Text = "";//clear Addressbox
            }
            catch (Exception ex)//handle any exception than may occur
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors(ex);
                AddressBox.Text = "";
            }
        }
        /// <summary>
        /// send text from MsgBox into server using the established connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!client.Connected)//check if client is connected  or not 
                {
                    throw new DisconnectedException();
                }
                if (MsgBox.Text != "")//if no message written then no data to send
                {

                    MsgBox.Text = string.Concat(NameBox.Text + " : ", MsgBox.Text);//concatinate user name and message written by him using saparation symbol 
                    network = client.GetStream();//handle client stream 
                    byte[] SendData = Encoding.UTF8.GetBytes(MsgBox.Text);//hold message in bytes
                    byte[] Header = BitConverter.GetBytes(SendData.Length);
                    network.Write(Header, 0, 2);
                    network.Write(SendData, 0, SendData.Length);//send data
                    MsgBox.Text = "";//clear message box    
                    MsgBox.Focus();//give message box foucs to write next message
                }
            }
            catch (DisconnectedException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PrintErrors(ex);
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
        /// <summary>
        /// Notify server that client will close.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client.Connected)
            {
                //client.Close();
                client.Client.Disconnect(true);
            }
        }
        /// <summary>
        /// Print exception message in Error.txt file in the application folder.
        /// </summary>
        /// <param name="ex">
        /// exception that occured
        /// </param>
        private void PrintErrors(Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt", true))
            {
                stream.WriteLine("Date : " + DateTime.Now.ToLocalTime());
                stream.WriteLine("Stack trace :");
                stream.WriteLine(ex.StackTrace);
                stream.WriteLine("Message :");
                stream.WriteLine(ex.Message);
                stream.WriteLine("---------------------------------------------------------------------------------------------------------------");
            }
        }
        /// <summary>
        /// Print exception message in Error.txt file in the application folder.
        /// </summary>
        /// <param name="Message">
        /// error message
        /// </param>
        /// <param name="ex">
        /// exception to write it's stack trace
        /// </param>
        private void PrintErrors(string Message, Exception ex)
        {
            string ErrorPath = System.IO.Directory.GetParent(@"..\..\..\").FullName;
            using (StreamWriter stream = new StreamWriter(ErrorPath + @"\Error.txt", true))
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
