using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class MainForm : Form
    {
        private ChatClientConnection clientInstance = new ChatClientConnection();

        // ��������� ��� ����, ����� �������� ����� ����������� �� ������� ������
        public delegate void EstablishConnectionCallback();
        // ��������� ��� ����, ����� �������� ����� ����������� �� ������� ������
        public delegate void UpdateLogCallback(string strMessage);
        // ����� ��� ����, ����� ������� ������� �������� ���������� � ������ ��������� �� ������ 
        public delegate void CloseConnectionCallback(string strReason);

        public MainForm()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
            clientInstance.clientConnectedCallback = this.OnConnectedWrapper;
            clientInstance.clientUpdatedCallback = this.OnUpdateWrapper;
            clientInstance.clientDisconnectedCallback= this.OnDisconnectedWrapper;
        }

        // ���������� ������� ��� ������ �� ����������
        public void OnApplicationExit(object sender, EventArgs e)
        {
            clientInstance.clientConnectedCallback = null;
            clientInstance.clientUpdatedCallback = null;
            clientInstance.clientDisconnectedCallback = null;
            clientInstance.CloseConnectionWithMessage("Exit on application closed");
        }

        private void OnDisconnectedWrapper(string text)
        {
            this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { text });
        }

        private void OnConnectedWrapper()
        {
            this.Invoke(new EstablishConnectionCallback(()=> { RefreshForm(false); }), new object[] { });
        }

        private void OnUpdateWrapper(string text)
        {
            this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { text });
        }

        private void RefreshForm(bool isConnectionEnabled)
        {
            txtIp.Enabled = isConnectionEnabled;
            txtAppartment.Enabled = isConnectionEnabled;
            txtPassword.Enabled = isConnectionEnabled;
            txtMessage.Enabled = !isConnectionEnabled;
            btnSend.Enabled = !isConnectionEnabled;
            btnConnect.Text = isConnectionEnabled ? "Connect" : "Disconnect";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // ���� ������������ ����� ������������, �� ���� ��� �� ���������
            if (clientInstance.Connected)
            {
                clientInstance.CloseConnectionWithMessage("Disconnected at user's request.");
            }
            // ����������
            else
            {
                clientInstance.InitializeConnection(txtIp.Text, txtAppartment.Text, txtPassword.Text);
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
            RefreshForm(true);
        }

        // ���������� ��������� ��������� �� ������
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                clientInstance.sendMessage(txtMessage.Text);
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