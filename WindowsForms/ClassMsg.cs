using System;

namespace MsgClass
{

    //���������������ʶ��
    public enum MsgCommand
    {
        NONE,
        CONNECT = 1,
        LOGIN = 3,//�û���¼

        HEART_BEAT_RES = 6,//������Ӧ

        UPDATE_NODE_NUM = 7,//���ע��

        COLLECTER_INFO_RES = 10,//�����Ϣ��ѯ�ķ������ݰ�
        

    }

    //�������������
    public class Conmmend{

        public static string CONNECTION_RES = "<c>02</c><info></info>";//������Ӧ

        public static string LOGIN_MSG_RES = "<c>04</c><info></info>";//��¼��Ӧ


        public static string HEART_BEAT = "<c>05</c><info></info>";//��������


        public static string UPDATE_NODE_NUM_RES = "<c>08</c><info></info>";//���ע����Ӧ

        public static string GET_ALL_COLLECTER_INFO = "<c>09</c><info>All</info>"; //��ѯ���е��ɼ�����Ϣ  


    }
   

}
