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
                // Преобразуем значение из txtIp в объект типа IPAddress
                IPAddress ipAddr = IPAddress.Parse(txtIp.Text);
                // Создается новый экземпляр объекта ChatServer
                ChatServer mainServer = new ChatServer(ipAddr);
                // Подключаем обработчик события ChatMessageHandler к mainServer_ChatMessageHandler
                ChatServer.ChatMessageHandler += new ChatMessageEventHandler(mainServer_ChatMessageHandler);
                // Начинается прослушивание соединений 
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
            // Вызывается метод UpdateStatus с аргументом е.EventMessage 
            this.Invoke(new UpdateStatusCallback(this.UpdateStatus), new object[] { e.EventMessage });
        }

        private void UpdateStatus(string strMessage)
        {
            // Обновляется журнал сообщений
            txtLog.AppendText(strMessage + "\r\n");
        }
    }
}