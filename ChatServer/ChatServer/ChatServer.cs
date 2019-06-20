using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;

namespace ChatServer
{
    // Аргументыы события ChatMessageEventArgs
    public class ChatMessageEventArgs : EventArgs
    {
        // Сообщение о произошедшем событии
        private string EventMsg;
         
        // Получение сообщения о событии
        public string EventMessage
        {
            get
            {
                return EventMsg;
            }
            
        }

        // Конструктор для установки сообщения о событии
        public ChatMessageEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }


    // Для визуального отображения о сообщениях на сервере
    public delegate void ChatMessageEventHandler(object sender, ChatMessageEventArgs e);

    class ChatServer
    {
        public const string AdministratorName = "Administrator";
        public const int ServerPort = 1986;
        // Пара ключ-значение для юзера и соединения
        public static Hashtable htUsers = new Hashtable(30); // максимум 30 пользователей одновременнно 
        public static Hashtable htConnections = new Hashtable(30);
        // Сохранит переданный IP-адрес
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        // Событие и его аргумент будут уведомлять форму, когда пользователь совершил какое-то действие
        public static event ChatMessageEventHandler ChatMessageHandler;

        // Конструктор устанавливает IP-адрес на тот, который был получен экземпляром объекта
        public ChatServer(IPAddress address)
        {
            ipAddress = address;
        }

        public void CloseServer()
        {
            //SendAdminMessage("Server is going to shut down");
            tlsClient.Stop();
            ChatServer.htUsers.Clear();
            ChatServer.htConnections.Clear();
        }
        
        // Поток, который будет держать данные соединения
        private Thread thrListener;
        
        // Объект TCP, который соответсвует соединению
        private TcpListener tlsClient;

        // Отслеживание соединений
        bool ServRunning = false;

        // Добавление пользователя в хеш-таблицы
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            ChatServer.htUsers.Add(strUsername, tcpUser);
            ChatServer.htConnections.Add(tcpUser, strUsername);

            // Сообщаем о новом подключении всем остальным пользователям и форме сервера
            SendAdminMessage("Apartment №" + htConnections[tcpUser] + " has joined us");
        }

        // Удаление пользователя из хеш-таблиц
        public static void RemoveUser(TcpClient tcpUser)
        {
            // Если пользователь содержится
            if (htConnections[tcpUser] != null)
            {
                SendAdminMessage("Apartment №" + htConnections[tcpUser] + " has left us");

                // Удаление пользователя
                ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
                ChatServer.htConnections.Remove(tcpUser);
            }
        }

        // Вызывается, когда мы хотим вызвать событие ChatMessageEventArgs
        public static void OnStatusChanged(ChatMessageEventArgs e)
        {
            // Вызов события e
            ChatMessageHandler?.Invoke(null, e);
        }

        private static void BroadcastMessage(string fullMessage)
        {
            // Создаем массив ТСР-клиентов; размер равен количеству пользователей в хэш-таблице
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            // заполняем массив подсоединенными пользователями в данный момент
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // Цикл по всем пользователям
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Пытаемся отправить сообщение каждому пользователю
                try
                {
                    // Проверка, что сообщение не пустое
                    if (fullMessage.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Отправляем сообщение 
                    var swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(fullMessage);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Иначе удаляем этого пользователя
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        // Отправка сообщения администратора
        public static void SendAdminMessage(string Message)
        {
            string msg = AdministratorName + ": " + Message;
            var e = new ChatMessageEventArgs(msg);
            OnStatusChanged(e);
            BroadcastMessage(msg);
        }
        
        // Отправляем сообщение от одного пользователя всем остальным
        public static void SendUserMessage(string From, string Message)
        {
            string msg = From + " says: " + Message;
            var e = new ChatMessageEventArgs(msg);
            OnStatusChanged(e);
            BroadcastMessage(msg);
        }

        public void StartListening()
        {
            // Создание объекта прослушивателя TCP с использованием IP-адреса сервера и указанного порта
            IPAddress ipaLocal = ipAddress;
            tlsClient = new TcpListener(ipaLocal, ServerPort);

            // Запускаем TCP прослушиватель и ищем соединения
            tlsClient.Start();
            
            // вспомогательная переменная для цикла
            ServRunning = true;
            
            // Запускаем новый поток, в котором находится слушатель
            thrListener = new Thread(KeepListening);
            thrListener.Start();
        }

        private void KeepListening()
        {
            try
            {
                // Пока работает сервер
                while (ServRunning == true)
                {
                    // Разрешить ожидающее соединение
                    tcpClient = tlsClient.AcceptTcpClient();
                    // Создать новый экземпляр Connection
                    ChatClientConnection newConnection = new ChatClientConnection(tcpClient);
                }
            }
            catch
            { }
        }
    }
}
