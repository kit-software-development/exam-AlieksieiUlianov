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
    // ���������� ������� ChatMessageEventArgs
    public class ChatMessageEventArgs : EventArgs
    {
        // ��������� � ������������ �������
        private string EventMsg;
         
        // ��������� ��������� � �������
        public string EventMessage
        {
            get
            {
                return EventMsg;
            }
            
        }

        // ����������� ��� ��������� ��������� � �������
        public ChatMessageEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }


    // ��� ����������� ����������� � ���������� �� �������
    public delegate void ChatMessageEventHandler(object sender, ChatMessageEventArgs e);

    class ChatServer
    {
        // ���� ����-�������� ��� ����� � ����������
        public static Hashtable htUsers = new Hashtable(30); // �������� 30 ������������� ������������� 
        public static Hashtable htConnections = new Hashtable(30);
        // �������� ���������� IP-�����
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        // ������� � ��� �������� ����� ���������� �����, ����� ������������ �������� �����-�� ��������
        public static event ChatMessageEventHandler ChatMessageHandler;
        private static ChatMessageEventArgs e;

        // ����������� ������������� IP-����� �� ���, ������� ��� ������� ����������� �������
        public ChatServer(IPAddress address)
        {
            ipAddress = address;
        }
        
        // �����, ������� ����� ������� ������ ����������
        private Thread thrListener;
        
        // ������ TCP, ������� ������������ ����������
        private TcpListener tlsClient;

        // ������������ ����������
        bool ServRunning = false;

        // ���������� ������������ � ���-�������
        public static void AddUser(TcpClient tcpUser, string strUsername)
        {
            ChatServer.htUsers.Add(strUsername, tcpUser);
            ChatServer.htConnections.Add(tcpUser, strUsername);

            // �������� � ����� ����������� ���� ��������� ������������� � ����� �������
            SendAdminMessage("Apartment �" + htConnections[tcpUser] + " has joined us");
        }

        // �������� ������������ �� ���-������
        public static void RemoveUser(TcpClient tcpUser)
        {
            // ���� ������������ ����������
            if (htConnections[tcpUser] != null)
            {
                SendAdminMessage("Apartment �" + htConnections[tcpUser] + " has left us");

                // �������� ������������
                ChatServer.htUsers.Remove(ChatServer.htConnections[tcpUser]);
                ChatServer.htConnections.Remove(tcpUser);
            }
        }

        // ����������, ����� �� ����� ������� ������� ChatMessageEventArgs
        public static void OnStatusChanged(ChatMessageEventArgs e)
        {
            // ����� ������� e, ���� ��� �� null
            ChatMessageHandler?.Invoke(null, e);
        }

        // �������� ��������� ��������������
        public static void SendAdminMessage(string Message)
        {
            StreamWriter swSenderSender;

            e = new ChatMessageEventArgs("Administrator: " + Message);
            OnStatusChanged(e);
            
            // ������� ������ ���-��������; ������ ����� ���������� ������������� � ���-�������
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            // ��������� ������ ��������������� �������������� � ������ ������
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // ���� �� ���� �������������
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // �������� ��������� ��������� ������� ������������
                try
                {
                    // ��������, ��� ��������� �� ������
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // ���������� ��������� 
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine("Administrator: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // ����� ������� ����� ������������
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }
        
        // ���������� ��������� �� ������ ������������ ���� ���������
        public static void SendMessage(string From, string Message)
        {
            StreamWriter swSenderSender;

            e = new ChatMessageEventArgs(From + " says: " + Message);
            OnStatusChanged(e);
            
            // ������� ������ ���-��������; ������ ����� ���������� ������������� � ���-�������
            TcpClient[] tcpClients = new TcpClient[ChatServer.htUsers.Count];
            // ��������� ������ ������������� �������������� � ������ ������
            ChatServer.htUsers.Values.CopyTo(tcpClients, 0);
            // ���� �� ��� ��������
            for (int i = 0; i < tcpClients.Length; i++)
            {
                // �������� ��������� ��������� ������� ������������
                try
                {
                    // ��������, ��� ��������� �� ������
                    if (Message.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // ���������� ��������� 
                    swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(From + " says: " + Message);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // ����� ������� ����� ������������
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        public void StartListening()
        {
            // �������� ������� �������������� TCP � �������������� IP-������ ������� � ���������� �����
            IPAddress ipaLocal = ipAddress;
            tlsClient = new TcpListener(ipaLocal, 1986);

            // ��������� TCP �������������� � ���� ����������
            tlsClient.Start();
            
            // ��������������� ���������� ��� �����
            ServRunning = true;
            
            // ��������� ����� �����, � ������� ��������� ���������
            thrListener = new Thread(KeepListening);
            thrListener.Start();
        }

        private void KeepListening()
        {
            // ���� �������� ������
            while (ServRunning == true)
            {
                // ��������� ��������� ����������
                tcpClient = tlsClient.AcceptTcpClient();
                // ������� ����� ��������� Connection
                Connection newConnection = new Connection(tcpClient);
            }
        }
    }
    
    // ���� ����� ������������ ����������; ����� ������� ����������, ������� ����� ������������ �������������
    class Connection
    {
        TcpClient tcpClient;
        // �����, ������� ����� ���������� ���������� �������
        private Thread thrSender;
        private StreamReader srReceiver;
        private StreamWriter swSender;
        private string currUser;
        private string currPass;
        private string strResponse;
        
        // ����������� ������ ��������� ���������� TCP
        public Connection(TcpClient tcpCon)
        {
            tcpClient = tcpCon;
            // �����, ����������� ������� � ��������� ���������
            // ����� �������� ����� AcceptClient ()
            thrSender = new Thread(AcceptClient);
            thrSender.Start();
        }

        private void CloseConnection()
        {
            // ������� �������� � ������ ������ �������
            tcpClient.Close();
            srReceiver.Close();
            swSender.Close();
        }

        // ��� �������� ������ �������
        private void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());
            
            // ���������� ���������� ���������� �� �������
            var help_var = srReceiver.ReadLine();
            currUser = help_var.Substring(0, help_var.IndexOf("|"));
            currPass = help_var.Substring(help_var.IndexOf("|")+1);
            // �������� ���������� �� ������� ���������� 
            if (currUser != "")
            {
                // ������ ������������ ��� ���������
                if (ChatServer.htUsers.Contains(currUser) == true)
                {
                    // 0 ����� ��� ����, ����� ��������, ��� ���������� �� ������������
                    swSender.WriteLine("0|This username already exists.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else if (currUser == "Administrator") // ������������ �� ����� ����� ����� Administrator
                {
                    // 0 ����� ��� ����, ����� ��������, ��� ���������� �� ������������
                    swSender.WriteLine("0|This username is reserved.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else
                {
                    
                    string connecttionString = GetConnnectionString();
                    // ������������ ���������� � ��
                    using (SqlConnection connection = new SqlConnection(connecttionString))
                    {
                        connection.Open();
                        string query = "SELECT Apartment_ID,Password FROM [Apartment_Security] WHERE Apartment_ID=@un";
                        // �������� SQL �������
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@un", currUser);
                            // ��������� ����������� ���� �������
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                bool userCorrect = false;
                                while (reader.Read())
                                {
                                    Int32 usrName = reader.GetInt32(0);
                                    string usrPass = reader.GetString(1);
                                    // ��������, ��� ����� � ������ �������� ������������ ��������� � 
                                    // ���������������� ���������� �� ������� ��
                                    if (usrName.ToString() == currUser && usrPass == currPass)
                                    {
                                        userCorrect = true;
                                        // 1 ����� ��� ����, ����� ���������� �������� ����������
                                        swSender.WriteLine("1");
                                        swSender.Flush();

                                        // ��������� ������������ � ���-������� � �������� ������������ ��������� �� ����
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
                // ���� ���������� �������
                while ((strResponse = srReceiver.ReadLine()) != "")
                {
                    // ���� ��� �������, ������� ������������
                    if (strResponse == null)
                    {
                        ChatServer.RemoveUser(tcpClient);
                    }
                    else
                    {
                        // ����� ��������� ��������� ���� ������ �������������
                        ChatServer.SendMessage(currUser, strResponse);
                    }
                }
            }
            catch
            {
                // ���� �������� �����-�� ������ ��� �������� ���������, ������� ������������ 
                ChatServer.RemoveUser(tcpClient);
            }
        }

        private static string GetConnnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                //1. ������� ��������
                DataSource = "(localdb)\\MSSQLLocalDB",
                //2. ������� ����������������� �����
                AttachDBFilename = Path.GetFullPath("Database.mdf"),
                IntegratedSecurity = true
            };
            return builder.ConnectionString;
        }

    }
}
