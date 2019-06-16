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
        // Необходим для того, чтобы обновить форму сообщениями из другого потока
        private delegate void UpdateLogCallback(string strMessage);
        // Нужен для того, чтобы закрыть объекты текущего соединения и выдать сообщение об ошибке 
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;

        public MainForm()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
        }

        // Обработчик события для выхода из приложения
        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Закрываем соедиения и потоки
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpClient.Close();
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Если пользователь хочет подключиться, но пока еще не подключен
            if (Connected == false)
            {
                // Подключение
                InitializeConnection();
            }
            // Отключение
            else 
            {
                CloseConnection("Disconnected at user's request.");
            }
        }

        private void InitializeConnection()
        {
            // Преобразование IP-адреса из TextBox в объект типа IPAddress
            ipAddr = IPAddress.Parse(txtIp.Text);
            // Запускаем новое TCP соединение с сервером чата
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
            
            // Серверу посылается введенные логин и паоль
            swSender = new StreamWriter(tcpClient.GetStream());
            swSender.WriteLine(txtAppartment.Text + "|" + txtPassword.Text);
            swSender.Flush();
            
            // Запускаем поток для получения сообщений и дальнейшего общения
            // thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging = new Thread(ReceiveMessages);

            thrMessaging.Start();
        }

        private void ReceiveMessages()
        {
            // Получаем ответ от сервера
            srReceiver = new StreamReader(tcpClient.GetStream());
            string ConResponse = srReceiver.ReadLine();
            // Если первый символ = 1, то соединение прошло успешно
            if (ConResponse[0] == '1')
            {
                // Оповещение, что соединение прошло успешно
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Connected Successfully!" });
            }
            else
            {
                string Reason = "Not Connected: ";
                // Добавление причиниы неудачного подключения
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                // Оповещение, что соединение прошло не успешно
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                return;
            }
            // Пока мы успешно подключены, читаем входящие линии с сервера
            while (Connected)
            {
                try
                {
                    var inputText = srReceiver.ReadLine();
                    // Показать сообщения в TextBox
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { inputText });
                }
                catch(Exception)
                {
                    // если возникает ошибка, то пользователь отключается и выдается сообщение об ошибке
                    // Если нажато disconnected, то сообщение не выдается
                    if (Connected)
                    {
                        this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { "There are some errors with connection!" });
                    }
                    return;
                }
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

            txtIp.Enabled = true;
            txtAppartment.Enabled = true;
            txtPassword.Enabled = true;
            txtMessage.Enabled = false;
            btnSend.Enabled = false;
            btnConnect.Text = "Connect";

            // Закрываем объекты
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpClient.Close();
        }

        // Отправляет набранное сообщение на сервер
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