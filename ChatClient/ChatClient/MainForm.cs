using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatClient
{
    public partial class MainForm : Form
    {
        private string UserName = "Unknown";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpClient;
        // ��������� ��� ����, ����� �������� ����� ����������� �� ������� ������
        private delegate void UpdateLogCallback(string strMessage);
        // ����� ��� ����, ����� ������� ������� �������� ���������� � ������ ��������� �� ������ 
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;

        public MainForm()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        // ���������� ������� ��� ������ �� ����������
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // ��������� ��������� � ������
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpClient.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // ���� ������������ ����� ������������, �� ���� ��� �� ���������
            if (Connected == false)
            {
                // �����������
                InitializeConnection();
            }
            // ����������
            else 
            {
                CloseConnection("Disconnected at user's request.");
            }
        }

        private void InitializeConnection()
        {
            // �������������� IP-������ �� TextBox � ������ ���� IPAddress
            ipAddr = IPAddress.Parse(txtIp.Text);
            // ��������� ����� TCP ���������� � �������� ����
            tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect(ipAddr, 1986);
            }
            catch
            {
                UpdateLog("Server is not accessable!");
                return;
            }

            Connected = true;

            UserName = txtAppartment.Text;

            txtIp.Enabled = false;
            txtAppartment.Enabled = false;
            txtPassword.Enabled = false;
            txtMessage.Enabled = true;
            btnSend.Enabled = true;
            btnConnect.Text = "Disconnect";
            
            // ������� ���������� ��������� ����� � �����
            swSender = new StreamWriter(tcpClient.GetStream());
            swSender.WriteLine(txtAppartment.Text + "|" + txtPassword.Text);
            swSender.Flush();
            
            // ��������� ����� ��� ��������� ��������� � ����������� �������
            // thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging = new Thread(ReceiveMessages);

            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            // �������� ����� �� �������
            srReceiver = new StreamReader(tcpClient.GetStream());
            string ConResponse = srReceiver.ReadLine();
            // ���� ������ ������ = 1, �� ���������� ������ �������
            if (ConResponse[0] == '1')
            {
                // ����������, ��� ���������� ������ �������
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else
            {
                string Reason = "Not Connected: ";
                // ���������� �������� ���������� �����������
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // ����������, ��� ���������� ������ �� �������
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                return;
            }
            // ���� �� ������� ����������, ������ �������� ����� � �������
            while (Connected)
            {
                try
                {
                    var inputText = srReceiver.ReadLine();
                    // �������� ��������� � TextBox
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { inputText });
                }
                catch(Exception)
                {
                    // ���� ��������� ������, �� ������������ ����������� � �������� ��������� �� ������
                    // ���� ������ disconnected, �� ��������� �� ��������
                    if (Connected)
                    {
                        this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { "There are some errors with connection!" });
                    }
                    return;
                }
            }
        }
        
        // ���� ����� ���������� �� ������� ������ ��� ���������� ������� TextBox
        private void UpdateLog(string strMessage)
        {
            // ����������� ����� ����� ������������ TextBox ���� ������ ���
            txtLog.AppendText(strMessage + "\r\n");
        }

        // ��������� �������� ����������
        private void CloseConnection(string Reason)
        {
            // ������� ������ ��������� �� �����������
            txtLog.AppendText(Reason + "\r\n");

            txtIp.Enabled = true;
            txtAppartment.Enabled = true;
            txtPassword.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // ��������� �������
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpClient.Close();
        }

        // ���������� ��������� ��������� �� ������
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                swSender.WriteLine(txtMessage.Text);
                swSender.Flush();
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        // �������� ���������, ���� ������ ������ Send 
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // �������� ���������, ���� ������ ������� Enter 
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}