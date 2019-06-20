using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace ChatClient
{
    class ChatClientConnection
    {
        private TcpClient tcpClient;
        private StreamWriter swSender;
        private StreamReader srReceiver;

        public bool Connected { get { return mConnected; } }
        private bool mConnected = false;

        public string UserName;
        public string UserPassword;
        public string ServerIP;

        private Thread thrMessaging;

        // Необходим для того, чтобы обновить форму сообщениями из другого потока
        public delegate void ConnectionEstablishedCallback();
        // Необходим для того, чтобы обновить форму сообщениями из другого потока
        public delegate void UpdateLogCallback(string strMessage);
        // Нужен для того, чтобы закрыть объекты текущего соединения и выдать сообщение об ошибке 
        public delegate void ConnectionClosedCallback(string strReason);

        public ConnectionEstablishedCallback clientConnectedCallback;
        public UpdateLogCallback             clientUpdatedCallback;
        public ConnectionClosedCallback      clientDisconnectedCallback;


        public void InitializeConnection(string serverIP, string username, string userpassword)
        {
            tcpClient = new TcpClient();
            IPAddress ipAddr = IPAddress.Parse(serverIP);
            try
            {
                tcpClient.Connect(ipAddr, 1986);
            }
            catch
            {
                clientUpdatedCallback?.Invoke("Server is not accessable!");
                return;
            }
            UserName = username;
            UserPassword = userpassword;
            ServerIP = serverIP;
            
            // Запускаем поток для получения сообщений и дальнейшего общения
            // thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging = new Thread(ChatClientInteraction);

            thrMessaging.Start();
            mConnected = true;
            clientConnectedCallback?.Invoke();
        }

        private void ChatClientInteraction()
        {
            // Серверу посылается введенные логин и паоль
            swSender = new StreamWriter(tcpClient.GetStream());
            srReceiver = new StreamReader(tcpClient.GetStream());

            swSender.WriteLine(UserName + "|" + UserPassword);
            swSender.Flush();
            // Получаем ответ от сервера
            string ConResponse;
            try
            {
                ConResponse = srReceiver.ReadLine();
            }
            catch
            {
                CloseConnectionWithMessage("Error getting message from server");
                return;
            }
            Regex filter = new Regex(@"^(\d+)\|(.+)$");
            var match = filter.Match(ConResponse);
            if (!match.Success)
            {
                CloseConnectionWithMessage("Got wrong message from server, disconnecting...");
                return;
            }
            if(match.Groups[1].Value != "0")
            {
                string closeConnectionMsg = "Connection error (" + match.Groups[1].Value + ") " + match.Groups[2].Value;
                CloseConnectionWithMessage(closeConnectionMsg);
                return;
            }
            clientUpdatedCallback("Connected successfully");

            // Пока мы успешно подключены, читаем входящие линии с сервера
            while (mConnected)
            {
                try
                {
                    string inputText;
                    while ((inputText = srReceiver.ReadLine()) != null)
                    {
                        if(inputText == "")
                        {
                            continue;
                        }
                        clientUpdatedCallback(inputText);
                    }
                }
                catch
                {
                    // если возникает ошибка, то пользователь отключается и выдается сообщение об ошибке
                    // Если нажато disconnected, то сообщение не выдается
                    if (mConnected)
                    {
                        CloseConnectionWithMessage("Error while server interaction");
                    }
                    return;
                }
            }
        }

        public void sendMessage(string message)
        {
            swSender.WriteLine(message);
            swSender.Flush();
        }

        public void CloseConnectionWithMessage(string message)
        {
            clientDisconnectedCallback?.Invoke(message);
            CloseConnection();
        }

        private void CloseConnection()
        {
            if (mConnected == true)
            {
                // Закрываем соедиения и потоки
                mConnected = false;
                swSender.Flush();
                swSender.Close();
                srReceiver.Close();
                tcpClient.Close();
                tcpClient = null;
            }
        }
    }
}
