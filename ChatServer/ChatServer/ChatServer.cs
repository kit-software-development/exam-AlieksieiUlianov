using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Data.SqlClient;

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
        // Пара ключ-значение для юзера и соединения
        public static Hashtable htUsers = new Hashtable(30); // максимум 30 пользователей одновременнно 
        public static Hashtable htConnections = new Hashtable(30);
        // Сохранит переданный IP-адрес
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        // Событие и его аргумент будут уведомлять форму, когда пользователь совершил какое-то действие
        public static event ChatMessageEventHandler ChatMessageHandler;
        private static ChatMessageEventArgs e;

        // Конструктор устанавливает IP-адрес на тот, который был получен экземпляром объекта
        public ChatServer(IPAddress address)
        {
            ipAddress = address;
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
            // Вызов события e, если оно не null
            ChatMessageHandler?.Invoke(null, e);
        }

        // Отправка сообщения администратора
        public static void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;

            e = new ChatMessageEventArgs("Administrator: " + Message);
            OnStatusChanged(e);
            
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
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Отправляем сообщение 
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Иначе удаляем этого пользователя
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }
        
        // Отправляем сообщение от одного пользователя всем остальным
        public static void SendMessage(string From, string Message)
        {
            StreamWriter swSenderSender;

            e = new ChatMessageEventArgs(From + " says: " + Message);
            OnStatusChanged(e);
            
            // Создаем массив ТСР-клиентов; размер равен количеству пользователей в хэш-таблице
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            // заполняем массив поделюченными пользователями в данный момент
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // Цикл по ТСР клиентам
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // Пытаемся отправить сообщение каждому пользователю
                try
                {
                    // Проверка, что сообщение не пустое
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // Отправляем сообщение 
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(From + " says: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // Иначе удаляем этого пользователя
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public void StartListening()
        {
            // Создание объекта прослушивателя TCP с использованием IP-адреса сервера и указанного порта
            IPAddress ipaLocal = ipAddress;
            tlsClient = new TcpListener(ipaLocal, 1986);

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
            // Пока работает сервер
            while (ServRunning == true)
            {
                // Разрешить ожидающее соединение
                tcpClient = tlsClient.AcceptTcpClient();
                // Создать новый экземпляр Connection
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }
    
    // Этот класс обрабатывает соединения; будет столько соединений, сколько будет подключенных пользователей
    class Connection
    {
        TcpClient tcpClient;
        // Поток, который будет отправлять информацию клиенту
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string currUser;
        private string currPass;
        private string strResponse;
        
        // Конструктор класса принимает соединение TCP
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            // Поток, принимающий клиента и ожидающий сообщения
            // Поток вызывает метод AcceptClient ()
            thrSender = new Thread(AcceptClient);
            thrSender.Start();
        }

        private void CloseConnection()
        {
            // Закрыть открытые в данный момент объекты
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        // При принятии нового клиента
        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());
            
            // Считывание присланной информации от клиента
            var help_var = srReceiver.ReadLine();
            currUser = help_var.Substring(0, help_var.IndexOf("|"));
            currPass = help_var.Substring(help_var.IndexOf("|")+1);
            // Проверка полученной от клиента информации 
            if (currUser != "")
            {
                // Данный пользователь уже подключен
                if (ChatServer.htUsers.Contains(currUser) == true)
                {
                    // 0 нужен для того, чтобы показать, что соединение не установилось
                    swSender.WriteLine("0|This username already exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else if (currUser == "Administrator") // Пользователь не может иметь логин Administrator
                {
                    // 0 нужен для того, чтобы показать, что соединение не установилось
                    swSender.WriteLine("0|This username is reserved.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else
                {
                    
                    string connecttionString = GetConnnectionString();
                    // Установление соединения с БД
                    using (SqlConnection connection = new SqlConnection(connecttionString))
                    {
                        connection.Open();
                        string query = "SELECT Apartment_ID,Password FROM [Apartment_Security] WHERE Apartment_ID=@un";
                        // Создание SQL команды
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@un", currUser);
                            // Получение результатов этой команды
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                bool userCorrect = false;
                                while (reader.Read())
                                {
                                    Int32 usrName = reader.GetInt32(0);
                                    string usrPass = reader.GetString(1);
                                    // Проверка, что логин и пароль текущего пользователя совпадают с 
                                    // соответствующими значениями из таблицы БД
                                    if (usrName.ToString() == currUser && usrPass == currPass)
                                    {
                                        userCorrect = true;
                                        // 1 нужна для того, чтобы обозначить успешное соединение
                                        swSender.WriteLine("1");
                                        swSender.Flush();

                                        // Добавляем пользователя в хеш-таблицы и начинаем прослушивать сообщения от него
                                        ChatServer.AddUser(tcpClient, currUser);
                                    }    
                                }
                                if (userCorrect == false)
                                {
                                    swSender.WriteLine("0|Wrong login or password!");
                                    swSender.Flush();
                                    CloseConnection();
                                    return;
                                }
                            }
                        }


                    }
                }
            }
            else
            {
                CloseConnection();
                return;
            }

            try
            {
                // Пока соединение активно
                while ((strResponse = srReceiver.ReadLine()) != "")
                {
                    // Если это неверно, удаляем пользователя
                    if (strResponse == null)
                    {
                        ChatServer.RemoveUser(tcpClient);
                    }
                    else
                    {
                        // Иначе рассылаем сообщение всем другим пользователям
                        ChatServer.SendMessage(currUser, strResponse);
                    }
                }
            }
            catch
            {
                // Если возникла какая-то ошибка при передаче сообщения, удаляем пользователя 
                ChatServer.RemoveUser(tcpClient);
            }
        }

        private static string GetConnnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                //1. указать источник
                DataSource = "(localdb)\\MSSQLLocalDB",
                //2. указать местораспалажения файла
                AttachDBFilename = Path.GetFullPath("Database.mdf"),
                IntegratedSecurity = true
            };
            return builder.ConnectionString;
        }

    }
}
