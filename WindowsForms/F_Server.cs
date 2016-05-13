using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.IO;

using System.Data.OleDb;

using Datawraper;
using System.Runtime.InteropServices;//INI�ļ���������
//using System.Text;
using InfoGetter;//�������
using MsgClass;

using DataModel;

namespace WindowsForms
{
    public partial class F_Server : Form
    {

        //localhost�ֶδ��滻
        private string database;
        private string userid;//��ȡ��½��Ϣ
        private string password;
        private string hostip;
        private string serverport;
        private string oracleip;
        private string oracleport = "1521";

        //private DataOp dataOp = null;

        //ʵ������Ϣ������
        infoGetter dataGetter = new infoGetter();



        //����ip��ַ����listview��idֵ
        private Dictionary<String, int> ViewListHash = new Dictionary<String, int>();




        //��¼ÿ�����ؽڵ����������
        //heartBeatResCounts , ��Ϊip��ַ����ֵΪ�洢�������ͣ�����״̬���ֵ䣬
        //���ֵ��У���һ����ʶ��sendTimes"�������ڶ���"recvFlag"��ʾ�Ƿ��յ���Ӧ�ı�ʶ
        private Dictionary<String, Dictionary<string, int>> heartBeatResCounts = new Dictionary<String, Dictionary<string, int> >();





        
        [DllImport("kernel32")]//�����ȡINI�ļ�API
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        
        /**     
         *  ��������ʼ��
         *  ���������ļ���Ϣ���������ݿ�����������ip��port
         * */

        [DllImport("user32.dll")]//ע���ȼ�API
        public static extern UInt32 RegisterHotKey(IntPtr hWnd, UInt32 id, UInt32 fsModifiers, UInt32 vk); 
        public F_Server()
        {
            InitializeComponent();
            RegisterHotKey(this.Handle, 247696411, 0, (UInt32)Keys.F11); //ע���ȼ�


            StringBuilder temp = new StringBuilder(255);//ʵ�����ɱ䳤�ȵ��ַ�������

            //��Server.INI�ļ�
         
            GetPrivateProfileString("ServerInfo", "Username", "���ݿ��û�����ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.userid = temp.ToString();
            GetPrivateProfileString("ServerInfo", "Password", "���ݿ��û������ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.password = temp.ToString();
            GetPrivateProfileString("ServerInfo", "Database", "���ݿ��ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.database = temp.ToString();
            GetPrivateProfileString("ServerInfo", "OracleIP", "���ݿ�IP��ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.oracleip = temp.ToString();
            GetPrivateProfileString("ServerInfo", "OraclePort", "���ݿ�˿ڶ�ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.oracleport = temp.ToString();
            GetPrivateProfileString("ServerInfo", "ServIP", "������IP��ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.hostip = temp.ToString();
            GetPrivateProfileString("ServerInfo", "ServPort", "�������˿ڶ�ȡ���� ", temp, 255, Environment.CurrentDirectory + "\\ServerInfo.ini");
            this.serverport = temp.ToString();

            UDPSocket_serv.LocalHost = this.hostip;//Ϊ������ָ��ip��ַ��Ĭ��Ϊ127.0.0.1
            UDPSocket_serv.LocalPort = int.Parse(this.serverport);//ָ������������port,Ĭ��Ϊ11000

            //�������ݿ����Ӿ��
            //this.dataOp = new DataOp(this.hostip , this.oraclePort , this.database , this.userid , this.password);

            //��ʼˢ���б�
            //Load_InfoList();

            //SysUser.Items.Clear();

            //�����¥ѡ����Ϣ
            //Load_BuildingInfo();

        }

       
        public F_Server(string ip, int port) {

            UDPSocket_serv.LocalHost = ip;//Ϊ������ָ��ip��ַ��Ĭ��Ϊ127.0.0.1
            UDPSocket_serv.LocalPort = port;//ָ������������port,Ĭ��Ϊ11000
            InitializeComponent();
        }
       

      

        
        private void Tool_Open(object sender, EventArgs e)
        {
            if (((ToolStripMenuItem)sender).Text == "��ʼ����")
            {
                ((ToolStripMenuItem)sender).Text = "��������";
                UDPSocket_serv.Active = true;

                //��������ʱ��
                heartBeat.Enabled = true;
                updateAllInfo.Enabled = true;
            }
            else
            {
                ((ToolStripMenuItem)sender).Text = "��ʼ����";
                UDPSocket_serv.Active = false;

                //�ر�����ʱ��
                heartBeat.Enabled = false;
                updateAllInfo.Enabled = false;
            }

            //
        }

        private void Server(bool IsServer)//��ʼ��ֹͣ����
        {

        }

        private void sockUDP1_DataArrival(byte[] Data, System.Net.IPAddress Ip, int Port)
        {
            DataArrivaldelegate outdelegate = new DataArrivaldelegate(DataArrival);
            this.BeginInvoke(outdelegate, new object[] { Data, Ip, Port }); 
        }
        private delegate void DataArrivaldelegate(byte[] Data, System.Net.IPAddress Ip, int Port);


        /**
         * �йܴ���Σ��������ݴ��䵽��ʱ�������ݴ���
         * 
         * 
         * 
         * */
        private void DataArrival(byte[] Data, System.Net.IPAddress Ip, int Port) //�������ݵ����Ĵ������
        {
            Console.WriteLine("IP : " + Ip + " PORT: " + Port);
     
            //UDPSocket_serv.Send(Ip, Port, Data);
            string msg = Encoding.UTF8.GetString(Data);//linux ����char[],,c#����utf8����

            //infoGetter infos = new infoGetter();

            //<head>id</head><data>datas</data>
            //������ת����enum���ͣ���
            try
            {
                MsgCommand msgkind = (MsgCommand)int.Parse(dataGetter.getHead(msg));
                switch (msgkind)
                {


                    case MsgCommand.CONNECT://1
                        {
                            //MessageBox.Show("1  Registering msg has come");
                            byte[] Response = System.Text.Encoding.Default.GetBytes(Conmmend.CONNECTION_RES);//��Ӧ����
                            UDPSocket_serv.Send(Ip, Port, Response);
                            //string data = infos.getData(msg);
                            //RegisterUser(data);
                        }
                        break;

                    case MsgCommand.LOGIN://3
                        {

                            try
                            {
                                UserLogin(msg, Ip, Port);

                                byte[] Response = System.Text.Encoding.Default.GetBytes(Conmmend.LOGIN_MSG_RES);//��Ӧ��¼
                                UDPSocket_serv.Send(Ip, Port, Response);


                            }
                            catch (Exception ex)
                            {

                                MessageBox.Show("user login : " + ex.Message);
                            }
                                                   
                        }
                        break;

                    case MsgCommand.HEART_BEAT_RES:
                        {
                            //Console.WriteLine("IP : " + Ip + "Heart Beat Response....");
                            heartBeatResCounts[Ip.ToString()]["RecvFlag"] = 1;//�޸ĸ�ip�ڵ����������Ӧ״̬
                        
                        }
                        break;

                    case MsgCommand.UPDATE_NODE_NUM:
                        {
                            Console.WriteLine("IP : " + Ip + " : Node number update....");
                            bool updateFlag = UpdateGatewayOfNode(msg, Ip);//���µ������������Ϣ
                            if (updateFlag) {

                                
                                byte[] Response = System.Text.Encoding.Default.GetBytes(Conmmend.UPDATE_NODE_NUM_RES);//��Ӧ������
                                UDPSocket_serv.Send(Ip, Port, Response);
                            }


                        }
                        break;

                    case MsgCommand.COLLECTER_INFO_RES:
                        {
                            Console.WriteLine("IP : " + Ip + " : " + msg);
                            bool updateFlag = UpdateHistoryOfInfo(msg);//���µ����ʷ��Ϣ��
                        
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            
            }
            
        }



        /**
        * 
        * ��ʼ����ʾ���ݿ�������Ϣ����UserList��ʾ
        * 
        * 
        * 
        * */
        private void Load_InfoList()
        {
            //Dictionary<String, int> ViewListHash = null;
            //List<Dictionary<String, String>> dataList = new List<Dictionary<string, string>>();

            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            List<Collection_t> dataList = new List<Collection_t>();
            //��ѯ���е����Ϣ
            OleDbDataReader data = dataOp.GetAllRowByTable("COLLECTOR_T");

            while (data.Read())
            {

                //Dictionary<String, String> dataListHash = new Dictionary<string,string>();
                Collection_t collection_t = new Collection_t(data[0].ToString(), data[1].ToString(),
                   data[2].ToString(), data[3].ToString(), data[4].ToString(), data[5].ToString(),
                   data[6].ToString(), data[7].ToString(), data[8].ToString(), data[9].ToString());


                //�õ�����������ص�ip��ַ
                OleDbDataReader data_gateway = dataOp.GetRowByIndex("GATEWAY_T", "GATEWAY_ID", collection_t.Gateway_id);

                ////��������id��ip��ַ��˿ڣ�
                while (data_gateway.Read())
                {
                    collection_t.Address = data_gateway[2].ToString();
                    collection_t.Port = data_gateway[3].ToString();
                    collection_t.Active_net = data_gateway[5].ToString();
                }


                dataList.Add(collection_t);

            }

            UpdateList(dataList);

            dataOp.Close();

        }





        
        /**
         * 
         * ���ص�¼
         * ���������󼴵�¼���������������ڷ������е���Ϣ
         * �����������ע��
         * 
         * 
         * */
        private void UserLogin(string msg , System.Net.IPAddress Ip, int Port)
        {

            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            string ip = Ip.ToString();
            //����ip��ַ��ѯ��¼
            OleDbDataReader data = dataOp.GetRowByIndex("GATEWAY_T", "ADDRESS", Ip.ToString());


            //������������
            if (data.HasRows)
            {
                string newvalue = "set address = '" + Ip.ToString() + "' , port = " + Port;
                int rownum = dataOp.UpdateOneRow("GATEWAY_T" , newvalue ,"ADDRESS" , Ip.ToString() );

                if (rownum != 1) {

                    Console.WriteLine("UpdateOneRow gateway_t error");
                }

                
                //�����б���������״̬
                //Load_InfoList();

            }
            else
            {
                //��������������

                //��ȡ���������
                int nextid = dataOp.GetMaxId("GATEWAY_T", "Gateway_id") + 1;

                //��������
                string value = nextid + ",'default', '" + Ip.ToString() + "'," + Port + ", 'default' , 'active'";

                int rownum = dataOp.InsertOneRow("GATEWAY_T", value);
            
                //׷�Ӽ�¼
                //Load_InfoList();

            }

            dataOp.Close();
            

        }



        

        /**
         * 
         * 
         * ���µ�����ڵ�����id
         * 
         * 
         * 
         * 
         * */

        private bool UpdateGatewayOfNode(string msg, System.Net.IPAddress Ip) 
        {

            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            bool flag = true;
            //�õ����ݰ��еĵ���
            string[] infos = dataGetter.getInfo(msg);
            foreach (string info in infos) 
            {

                Console.WriteLine(info);
                
                //�õ�������ip��id
                string gateway_id = dataOp.GetOneStrTypeByName("gateway_id" , "gateway_t", "address", Ip.ToString());

                //���ݵ��Ÿ�������id
                //string newvalue = "set address = '" + Ip.ToString() + "' , port = " + Port;
                string newvalue = "set gateway_id = " + gateway_id;
                int rownum = dataOp.UpdateOneRow("COLLECTOR_T", newvalue, "node_num", info);

                if (rownum != 1) {

                    flag = false;
                    Console.WriteLine("UpdateGatewayOfNode ERROR , ROW NUM AFFECTED IS NOT 1!");
                }

                
            }

            dataOp.Close();
            return flag;

            
        
        }


        /**
         * 
         * ���������ɼ��������ݰ�
         * ������ʷ���ݱ�
         * 
         * */
        private bool UpdateHistoryOfInfo(string msg)
        {
            bool flag = true;
            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            //node_ number | VOLTAGE | CURRANT | POWERS
            string[] infos = dataGetter.getInfo(msg);

            //���ݵ��Ų�����ʷ��Ϣ
            //��ȡ���������
            int nextid = dataOp.GetMaxId("HISTORY_T", "ID") + 1;

            ;
            //��������
            string value = nextid + ",1, " + infos[1] + "," + infos[2] + ", " +
                infos[3] + ",sysdate,'" + infos[0] +"'";

            int rownum = dataOp.InsertOneRow("HISTORY_T", value);
            
            if (rownum != 1)
            {
                flag = false;            
            
            }

            dataOp.Close();
            return flag;
        }




        private void F_Server_FormClosed(object sender, FormClosedEventArgs e)
        {
           

        }

        private void udpSocket1_DataArrival(byte[] Data, IPAddress Ip, int Port)
        {
            DataArrivaldelegate outdelegate = new DataArrivaldelegate(DataEvent);
            this.BeginInvoke(outdelegate, new object[] { Data, Ip, Port }); 

        }

        private void DataEvent(byte[] Data, System.Net.IPAddress Ip, int Port)
        {
            //MessageBox.Show(Encoding.Unicode.GetString(Data));
            //this.Text = Encoding.Unicode.GetString(Data);
            //MessageBox.Show(this.Text);
        }



        private void �˳�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Application.Exit();
            //System.Environment.Exit(0);//��ȫ�˳��ý���
            this.Close();

        }

        

        private void ����ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //�������ô���
            Login_Form login_forn = new Login_Form();
            login_forn.Text = "����������";

            if (login_forn.ShowDialog() == DialogResult.OK)
            {

                login_forn.Dispose();//�ͷ�login_form��������Դ
                MessageBox.Show("���óɹ���������Ч��");
                //Application.Run(new F_Server());
            }
            else
            {

                login_forn.Dispose();
                MessageBox.Show("�˳�����");
                //Application.Exit();
            }
        }

        //��Ǵ��嵱ǰ�Ƿ�����
        bool isHideFlag = true;
        private void ���ش���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
            isHideFlag = false;
        }


        //��д�ȼ�ע����Ϣѭ��F10
        //���췽����ע�ᣬ�˴���д������������
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            // m.WParam.ToInt32() Ҫ�� ע���ȼ�ʱ�ĵ�2������һ��
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == 247696411) //�ж��ȼ�
            {
                if (isHideFlag == true)
                {
                    this.Hide();
                    isHideFlag = false;
                }
                else
                {
                    this.Show();
                    isHideFlag = true;
                
                }
            }
            base.WndProc(ref m);
        }


        /**
         * 
         * ���������߳�
         * 
         * 
         * 
         * */
        private void heartBeat_Tick(object sender, EventArgs e)
        {

            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            //��ѯ���ݿ�����ע���������Ϣ���ҵ���ip��˿ڣ�����������
            OleDbDataReader data = dataOp.GetAllRowByTable("GATEWAY_T");
            //����data,�����ط���������
            try { 
            
            
                while (data.Read()) {

                    string ip = data[2].ToString();
                    string port = data[3].ToString();
                    string msg = Conmmend.HEART_BEAT;//����Э��

                    byte[] Msg = Encoding.UTF8.GetBytes(msg);

                    System.Net.IPAddress Ip = System.Net.IPAddress.Parse(ip);
                    int Port = int.Parse(port);

                    //Console.WriteLine("send to ip:" + ip + " port: " + port + " " + msg);
                    UDPSocket_serv.Send(Ip, Port, Msg);

                    try { 
                    
                        //heartBeatResCounts.TryGetValue(ip,out count);
                        int nowCount = heartBeatResCounts[ip]["SendTimes"]++;//��ip�ڵ㣬���ʹ�����1��

                        if (nowCount >= 5)
                        {
                            if (heartBeatResCounts[ip]["RecvFlag"] == 1)
                            {

                                //�ڵ�����أ��޸�״̬
                                string newvalue = "set state = 'Active' ";
                                int rownum = dataOp.UpdateOneRow("GATEWAY_T", newvalue, "ADDRESS", ip);

                                if (rownum != 1)
                                {

                                    Console.WriteLine("UpdateOneRow gateway states update error");
                                }

                            }
                            else
                            { 
                            
                                //�ڵ��������޸�״̬
                                string newvalue = "set state = 'Dead' ";
                                int rownum = dataOp.UpdateOneRow("GATEWAY_T", newvalue, "ADDRESS", ip);

                                if (rownum != 1)
                                {

                                    Console.WriteLine("UpdateOneRow gateway states update error");
                                }
                            }
                            heartBeatResCounts[ip]["RecvFlag"] = 0;//�޸ı�ʶ��������һ��
                            heartBeatResCounts[ip]["SendTimes"] = 1;//���¼���
                        }
                        
                        
                    }
                    catch(KeyNotFoundException ex1){//ע�Ⲷ���쳣˳��,�����һ���޴˼��� ���ʼ��

                        Console.WriteLine("ex1 : " + ex1.Message);

                        Dictionary<string, int> beatRes = new Dictionary<string, int>();
                        beatRes["SendTimes"] = 1;
                        beatRes["RecvFlag"] = 0;
                        heartBeatResCounts[ip] = beatRes;//��ʼ�����ʹ���
              
                    }
                       
                }
            }
            catch(Exception ex){


                Console.WriteLine("ex : " + ex.Message);
            }

            dataOp.Close();

        }



        private void updateInfo_Tick(object sender, EventArgs e)
        {



        }

        /**
         * 
         * ���е����Ϣ��ѯ�߳�
         * ��ѯ���е����Ϣ�����������ݿ�
         * 
         * */
        private void updateAllInfo_Tick(object sender, EventArgs e)
        {
            //byte[] Response = System.Text.Encoding.Default.GetBytes(Conmmend.GET_ALL_COLLECTER_INFO);//��Ӧ��¼
            
            
            //��ȡ��������ip��˿ں�
            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            //��ѯ���ݿ�����ע���������Ϣ���ҵ���ip��˿ڣ�
            OleDbDataReader data = dataOp.GetAllRowByTable("GATEWAY_T");
            //����data,�����ط������ݰ�
            try
            {

                while (data.Read())
                {

                    string ip = data[2].ToString();
                    string port = data[3].ToString();
                    string msg = Conmmend.GET_ALL_COLLECTER_INFO;//��ѯ���е����Ϣ

                    byte[] Msg = Encoding.UTF8.GetBytes(msg);

                    System.Net.IPAddress Ip = System.Net.IPAddress.Parse(ip);
                    int Port = int.Parse(port);

                    //Console.WriteLine("send to ip:" + ip + " port: " + port + " " + msg);
                    UDPSocket_serv.Send(Ip, Port, Msg);

                }
            }
            catch (Exception ex)
            {


                Console.WriteLine("ex : " + ex.Message);
            }

            dataOp.Close();

        }



        private void Load_BuildingInfo() 
        {
        
            //����¥���ݿ��building_t
            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            //�õ���¥���ƣ�Ĭ�ϲ��ظ���
            OleDbDataReader data = dataOp.GetAllRowByTable("BUILDING_T");

            //���ѡ���б�
            building.Items.Clear();
            //��ӵ�����ѡ���
            while (data.Read())
            {
                string building_name = data[1].ToString();
                building.Items.Add(building_name);
            }

            dataOp.Close();
            
        }




        /**
         * 
         * ��¥ѡ�񣬸ı䷿������
         * 
         * 
         * 
         * 
         * */
        private void building_SelectedIndexChanged(object sender, EventArgs e)
        {

            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);
            
            //��ȡ��¥��
            string building_name = building.Text;

            //��ȡ��¥id
            string building_id = dataOp.GetOneStrTypeByName("BUILDING_ID", "BUILDING_T", "BUILDING_NAME", building_name);

            //���ݴ�¥id��ѯ��¥��ķ���
            OleDbDataReader data = dataOp.GetRowByIndex("ROOM_T", "BUILDING_ID", building_id);

            //���·��������б�ֵ
            room.Items.Clear();
            while (data.Read())
            {

                string room_name = data[1].ToString();
                room.Items.Add(room_name);
            }

            dataOp.Close();
        }

        /**
         * 
         * ��ѯ��ť�����¼�
         * 
         * 
         * 
         * 
         * */
        private void search_Click(object sender, EventArgs e)
        {
            //����ѡ���ѯһ��������¼����ʾ�������б�
            DataOp dataOp = new DataOp(this.oracleip, this.oracleport, this.database, this.userid, this.password);

            //�õ���¥��
            string building_name = building.Text;
            string building_id = dataOp.GetOneStrTypeByName("BUILDING_ID", "BUILDING_T", "BUILDING_NAME", building_name);


            //�õ������
            string room_name = room.Text;
            string room_id = dataOp.GetOneStrTypeByName("ROOM_ID", "ROOM_T", "ROOM_NAME", room_name);

            //�õ�ѡ���ѯ�ĵ��״̬
            string collect_state = collectState.Text;


            //���ݴ�¥�ͷ���Ų�ѯ�����Ϣ
            string sql = null;


            //�ж�¥���뷿����Ƿ�Ϊ��,�Լ����״̬�����ɺ��ʵ�sql���
            if (building_id == null || room_id == null)
            {

                if (building_id == null && room_id == null)
                {
                    if (collect_state == "")
                    {


                        sql = "select * from COLLECTOR_T";
                    }
                    else
                    {

                        sql = "select * from COLLECTOR_T where STATE = '" + collect_state + "'";
                    }



                }
                else if (building_id != null)
                {
                    if (collect_state == "")
                    {


                        sql = "select * from COLLECTOR_T where BUILDING_ID = " + building_id;
                    }
                    else
                    {

                        sql = "select * from COLLECTOR_T where BUILDING_ID = " + building_id + " and STATE = '" + collect_state + "'";
                    }


                }
                else 
                {
                    if (collect_state == "")
                    {


                        sql = "select * from COLLECTOR_T where ROOM_ID = " + room_id;
                    }
                    else
                    {

                        sql = "select * from COLLECTOR_T where ROOM_ID = " + room_id + " and STATE = '" + collect_state + "'";
                    }
                

                
                }

                

            }
            else
            {
                if (collect_state == "")
                {


                    sql = "select * from COLLECTOR_T where BUILDING_ID = " + building_id + " and ROOM_ID = " + room_id;
                }
                else
                {

                    sql = "select * from COLLECTOR_T where BUILDING_ID = " + building_id + " and ROOM_ID = " + room_id +
                        " and STATE = '" + collect_state+"'";
                }
                
            
            }


            //����sql����ѯ�����Ϣ
            OleDbDataReader data = dataOp.GetRowsBySql(sql);

            List<Collection_t> dataList = new List<Collection_t>();//�ݴ����ݿ�����
            while (data.Read())
            {

                //Dictionary<String, String> dataListHash = new Dictionary<string,string>();
                Collection_t collection_t = new Collection_t(data[0].ToString(), data[1].ToString(),
                   data[2].ToString(), data[3].ToString(), data[4].ToString(), data[5].ToString(),
                   data[6].ToString(), data[7].ToString(), data[8].ToString(), data[9].ToString());


                //�õ�����������ص�ip��ַ
                OleDbDataReader data_gateway = dataOp.GetRowByIndex("GATEWAY_T", "GATEWAY_ID", collection_t.Gateway_id);

                ////��������id��ip��ַ��˿ڣ�
                while (data_gateway.Read())
                {
                    collection_t.Address = data_gateway[2].ToString();
                    collection_t.Port = data_gateway[3].ToString();
                    collection_t.Active_net = data_gateway[5].ToString();
                }


                dataList.Add(collection_t);

            }


            UpdateList(dataList);//������ʾ�����б�

            //���������б�
            dataOp.Close();


        }


        /**
         * 
         * ���ݵ������ģ���б���£���ʾ
         * 
         * 
         * 
         * */
        private void UpdateList(List<Collection_t> dataList)
        {
            SysUser.Items.Clear();
            SysUser.BeginUpdate();   //���ݸ��£�UI��ʱ����ֱ��EndUpdate���ƿؼ���������Ч������˸�������߼����ٶ� 


            //���½����б�
            foreach (Collection_t collec_t in dataList)
            {

                ListViewItem lvi = new ListViewItem();
                int i = dataList.IndexOf(collec_t);

                lvi.ImageIndex = i;
                lvi.Text = i.ToString();

                lvi.SubItems.Add(collec_t.Address);
                lvi.SubItems.Add(collec_t.Port);
                //lvi.SubItems.Add(collec_t.Node_id);//������
                lvi.SubItems.Add(collec_t.Node_num);//�����
                lvi.SubItems.Add(collec_t.Node_name);
                lvi.SubItems.Add(collec_t.Room_id);
                lvi.SubItems.Add(collec_t.Building_id);
                lvi.SubItems.Add(collec_t.State);
                lvi.SubItems.Add(collec_t.Active_net);

                SysUser.Items.Add(lvi);

            }

            SysUser.EndUpdate();  //�������ݴ���UI����һ���Ի��ơ�
        
        }


        //�ر�ǰѯ��
        private void F_Server_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("ȷ��Ҫ�ر�?", "��ʾ ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == DialogResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }





    }
}