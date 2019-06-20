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
        public const string AdministratorName = "Administrator";
        public const int ServerPort = 1986;
        // ���� ����-�������� ��� ����� � ����������
        public static Hashtable htUsers = new Hashtable(30); // �������� 30 ������������� ������������� 
        public static Hashtable htConnections = new Hashtable(30);
        // �������� ���������� IP-�����
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        // ������� � ��� �������� ����� ���������� �����, ����� ������������ �������� �����-�� ��������
        public static event ChatMessageEventHandler ChatMessageHandler;

        // ����������� ������������� IP-����� �� ���, ������� ��� ������� ����������� �������
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
            // ����� ������� e
            ChatMessageHandler?.Invoke(null, e);
        }

        private static void BroadcastMessage(string fullMessage)
        {
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
                    if (fullMessage.Trim() == "" || tcpClients[i] == null)
                    {
                        continue;
                    }
                    // ���������� ��������� 
                    var swSenderSender = new StreamWriter(tcpClients[i].GetStream());
                    swSenderSender.WriteLine(fullMessage);
                    swSenderSender.Flush();
                    swSenderSender = null;
                }
                catch // ����� ������� ����� ������������
                {
                    RemoveUser(tcpClients[i]);
                }
            }
        }

        // �������� ��������� ��������������
        public static void SendAdminMessage(string Message)
        {
            string msg = AdministratorName + ": " + Message;
            var e = new ChatMessageEventArgs(msg);
            OnStatusChanged(e);
            BroadcastMessage(msg);
        }
        
        // ���������� ��������� �� ������ ������������ ���� ���������
        public static void SendUserMessage(string From, string Message)
        {
            string msg = From + " says: " + Message;
            var e = new ChatMessageEventArgs(msg);
            OnStatusChanged(e);
            BroadcastMessage(msg);
        }

        public void StartListening()
        {
            // �������� ������� �������������� TCP � �������������� IP-������ ������� � ���������� �����
            IPAddress ipaLocal = ipAddress;
            tlsClient = new TcpListener(ipaLocal, ServerPort);

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
            try
            {
                // ���� �������� ������
                while (ServRunning == true)
                {
                    // ��������� ��������� ����������
                    tcpClient = tlsClient.AcceptTcpClient();
                    // ������� ����� ��������� Connection
                    ChatClientConnection newConnection = new ChatClientConnection(tcpClient);
                }
            }
            catch
            { }
        }
    }
}
