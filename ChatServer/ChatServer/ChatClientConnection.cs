using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
    using System.Collections;
    using System.Text.RegularExpressions;

namespace ChatServer
{
    // Этот класс обрабатывает соединения; будет столько соединений, сколько будет подключенных пользователей
    class ChatClientConnection
    {
        TcpClient tcpClient;
        // Поток, в котором будет обрабатываться сообщения клиента 
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;


        struct ValidationInfo
        {
            public int code;
            public string message;
        }

        UserInfo currUserInfo;

        // Конструктор класса принимает соединение TCP
        public ChatClientConnection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            // Поток, принимающий клиента и ожидающий сообщения
            // Поток вызывает метод AcceptClient ()
            thrSender = new Thread(ChatClientInteraction);
            thrSender.Start();
        }

        private void CloseConnection()
        {
            // Закрыть открытые в данный момент объекты
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        private UserInfo ExtractUserInfo(string inputData)
        {
            UserInfo usr = new UserInfo();
            Regex filter = new Regex(@"^(\d+)\|(.+)$");
            var match = filter.Match(inputData);
            if (!match.Success)
            {
                return usr;
            }
            usr.username = match.Groups[1].Value;
            usr.password = match.Groups[2].Value;
            return usr;
        }

        private ValidationInfo ValidateUserInfo(UserInfo info)
        {
            if(info.username == "")
            {
                return new ValidationInfo { code = 1, message = "Empty username" };
            }
            if(info.username == ChatServer.AdministratorName)
            {
                return new ValidationInfo { code = 2, message = "Reserved username" };
            }
            if (ChatServer.htUsers.Contains(info.username))
            {
                return new ValidationInfo { code = 3, message = "Duplicate username (user is already active in chat now)" };
            }
            if(!DatabaseProvider.IsValidUserInfo(info))
            {
                return new ValidationInfo { code = 4, message = "Wrong login or password" };
            }
            return new ValidationInfo { code = 0, message = "Access granted" };
        }

        // При принятии нового клиента
        private void ChatClientInteraction()
        {
            // Считывание присланной информации от клиента
            string help_var;
            try
            {
                help_var = srReceiver.ReadLine();
            }
            catch
            {
                CloseConnection();
                return;
            }
            var userInfo = ExtractUserInfo(help_var);
            var authRes = ValidateUserInfo(userInfo);

            string resultMessage = authRes.code.ToString() + "|" + authRes.message;
            swSender.WriteLine(resultMessage);
            swSender.Flush();
            if (authRes.code != 0)
            {
                CloseConnection();
                return;
            }
            currUserInfo = userInfo;
            ChatServer.AddUser(tcpClient, currUserInfo.username);
            try
            {
                string userMessage;
                // Пока соединение активно
                while (true)
                {
                    userMessage = srReceiver.ReadLine();
                    if(userMessage == null)
                    {
                        break;
                    }
                    if (userMessage == "")
                    {
                        continue;
                    }
                    // Как только мы получаем сообщение, передаем его серверу
                    ChatServer.SendUserMessage(currUserInfo.username, userMessage);
                }
            }
            finally
            {
                // Если возникла какая-то ошибка при передаче сообщения, удаляем пользователя 
                ChatServer.RemoveUser(tcpClient);
            }
        }

    }
}
