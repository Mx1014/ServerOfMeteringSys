using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading; 

namespace WindowsForms
{
    //�ؼ������뼴ʵ�������࣬������Ϊ�����������
    //������ΪUDPSocket_serv
    //�ͻ���ΪUDPSocket_clin
    public partial class UDPSocket : Component
    {
        private IPEndPoint ServerEndPoint = null;   //��������˵�
        private UdpClient UDP_Server = new UdpClient(); //�����������,Ҳ����UDP��Sockets
        public delegate void DataArrivalEventHandler(byte[] Data, IPAddress Ip, int Port);  //������һ���йܷ���
        public event DataArrivalEventHandler DataArrival;   //ͨ���й����ڿؼ��ж���һ���¼�

        
        private System.Threading.Thread thdUdp; //����һ���߳�
        
        /**
        private bool thread_on = false;
        [Browsable(true), Category("Local"), Description("��̨���������߳�")]
        public bool Thread_on
        {
            get { return thread_on; }
            set //�����Զ�ȡֵ
            {
                thread_on = value;
                if (thread_on) //��ֵΪTrueʱ
                {
                    thdUdp.Start();   //�򿪼���
                    MessageBox.Show("daadadad");

                }
                else
                {
                    thdUdp.Abort(); //�رս��������߳�
                }
            }
        }
        **/
        private string localHost = "127.0.0.1";
        [Browsable(true), Category("Local"), Description("����IP��ַ")]   //�ڡ����ԡ���������ʾlocalHost����
        public string LocalHost
        {
            get { return localHost; }
            set { localHost = value; }
        }

        private int localPort = 11000;
        [Browsable(true), Category("Local"), Description("���ض˿ں�")] //�ڡ����ԡ���������ʾlocalPort����
        public int LocalPort
        {
            get { return localPort; }
            set { localPort = value; }
        }

        private bool active = false;
        [Browsable(true), Category("Local"), Description("�������")]   //�ڡ����ԡ���������ʾactive����
        public bool Active
        {
            get { return active; }  
            set //�����Զ�ȡֵ
            { 
                active = value;
                if (active) //��ֵΪTrueʱ
                {
                    OpenSocket();   //�򿪼���
                }
                else
                {
                    CloseSocket();  //�رռ���
                }
            }
        }


        public UDPSocket()
        {
            InitializeComponent();
        }

        public UDPSocket(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        /*
         * ������������
         * Ϊÿһ���ͻ��˴����߳�
         * 
         * 
         * */
        protected void Listener()   //����
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(this.localHost), localPort);   //��IP��ַ�Ͷ˿ں�������˵�洢
            if (UDP_Server != null)
                UDP_Server.Close();
            UDP_Server = new UdpClient(localPort);  //����һ���µĶ˿ں�
            UDP_Server.Client.ReceiveBufferSize = 1000000000;   //���ջ�������С
            UDP_Server.Client.SendBufferSize = 1000000000;  //���ͻ�������С
            
            try
            {
                MessageBox.Show("udp�������������������߳�");
                thdUdp = new Thread(new ThreadStart(GetUDPData));   //����һ���̣߳�
                //this.thread_on = true;//ִ�е�ǰ�߳�
                thdUdp.Start(); //ִ�е�ǰ�߳�
                thdUdp.IsBackground = true;
                //udp���������һ�������̣߳�æ�򽫿ͻ�����Ϣ�ݴ滺��
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());  //��ʾ�̵߳Ĵ�����Ϣ
            }
        }

        private void GetUDPData()   //��ȡ��ǰ���յ���Ϣ
        {
            while (active)//ѭ�������ͻ�������
            {
                try
                {
                    byte[] Data = UDP_Server.Receive(ref ServerEndPoint);   //����ȡ��Զ����Ϣת���ɶ�������

                    if (DataArrival != null)
                    {
                        //MessageBox.Show(ServerEndPoint.Address.ToString() + ServerEndPoint.Port);
                        DataArrival(Data, ServerEndPoint.Address, ServerEndPoint.Port);
                    }
                    Thread.Sleep(0);
                }
                catch { }
            }
        }

        private void CallBackMethod(IAsyncResult ar)
        {
            //���첽״̬ar.AsyncState�У���ȡί�ж���
            DataArrivalEventHandler dn = (DataArrivalEventHandler)ar.AsyncState;
            //һ��ҪEndInvoke
            dn.EndInvoke(ar);
        }


        public void Send(System.Net.IPAddress Host, int Port, byte[] Data)
        {
            try
            {
                IPEndPoint server = new IPEndPoint(Host, Port);
                UDP_Server.Send(Data, Data.Length, server);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        } 

        private void OpenSocket()
        {
            Listener();
        }

        private void CloseSocket()
        {
            if (UDP_Server != null)
                UDP_Server.Close();
            if (thdUdp != null)
            {
                Thread.Sleep(30);
                thdUdp.Abort();
            }
        }
    }
}
