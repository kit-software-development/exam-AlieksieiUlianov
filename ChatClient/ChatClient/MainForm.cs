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

        // Необходим для того, чтобы обновить форму сообщениями из другого потока
        public delegate void EstablishConnectionCallback();
        // Необходим для того, чтобы обновить форму сообщениями из другого потока
        public delegate void UpdateLogCallback(string strMessage);
        // Нужен для того, чтобы закрыть объекты текущего соединения и выдать сообщение об ошибке 
        public delegate void CloseConnectionCallback(string strReason);

        public MainForm()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
            clientInstance.clientConnectedCallback = this.OnConnectedWrapper;
            clientInstance.clientUpdatedCallback = this.OnUpdateWrapper;
            clientInstance.clientDisconnectedCallback= this.OnDisconnectedWrapper;
        }

        // Обработчик события для выхода из приложения
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
            // Если пользователь хочет подключиться, но пока еще не подключен
            if (clientInstance.Connected)
            {
                clientInstance.CloseConnectionWithMessage("Disconnected at user's request.");
            }
            // Отключение
            else
            {
                clientInstance.InitializeConnection(txtIp.Text, txtAppartment.Text, txtPassword.Text);
            }
        }

        // Этот метод вызывается из другого потока для обновления журнала TextBox
        private void UpdateLog(string strMessage)
        {
            // Добавленный текст также прокручивает TextBox вниз каждый раз
            txtLog.AppendText(strMessage + "\r\n");
        }

        // Закрываем текуещее соединение
        private void CloseConnection(string Reason)
        {
            // Причина почему соединеие не установлено
            txtLog.AppendText(Reason + "\r\n");
            RefreshForm(true);
        }

        // Отправляет набранное сообщение на сервер
        private void SendMessage()
        {
            if (txtMessage.Lines.Length >= 1)
            {
                clientInstance.sendMessage(txtMessage.Text);
                txtMessage.Lines = null;
            }
            txtMessage.Text = "";
        }

        // Отправка сообщения, если нажата кнопка Send 
        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        // Отправка сообщения, если нажата клавиша Enter 
        private void txtMessage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}