using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace ChatServer
{
    public partial class MainForm : Form
    {
        private delegate void UpdateStatusCallback(string strMessage);

        public MainForm()
        {
            InitializeComponent();
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            try
            {
                // ����������� �������� �� txtIp � ������ ���� IPAddress
                IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
                // ��������� ����� ��������� ������� ChatServer
                ChatServer mainServer = new ChatServer(ipAddr);
                // ���������� ���������� ������� ChatMessageHandler � mainServer_ChatMessageHandler
                ChatServer.ChatMessageHandler += new ChatMessageEventHandler(mainServer_ChatMessageHandler);
                // ���������� ������������� ���������� 
                mainServer.StartListening();
                txtLog.AppendText("Monitoring for connections...\r\n");
                btnListen.Enabled = false;
            }
            catch
            {
                txtLog.AppendText("Error with creating server acceptor...\r\n");
                return;
            }
        }

        public void mainServer_ChatMessageHandler(object sender, ChatMessageEventArgs e)
        {
            // ���������� ����� UpdateStatus � ���������� �.EventMessage 
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // ����������� ������ ���������
            txtLog.AppendText(strMessage + "\r\n");
        }
    }
}