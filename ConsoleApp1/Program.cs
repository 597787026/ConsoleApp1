using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Data.SqlClient;//数据库

namespace ConsoleApp1
{
    class Program
    {
        public static byte[] StringToHex(string a)
        {
            string[] lineArray = a.Split(' ');//split
            int length = lineArray.Length;//length of LineAray

            for (int i = 0; i < length; i++)//去掉最后面的空白，保险起见
            {
                if (lineArray[i] == "")
                {
                    length--;
                }
            }
            byte[] bytebuffer = new byte[length];//定义一个byte数组存放转化后的HEX

            int byteCount = 0;
            for (int i = 0; i < length; i++)
            {
                byte[] bt = Encoding.Default.GetBytes(lineArray[i]);
                int decnum = 0;
                if (lineArray[i] == "") continue;
                else
                {
                    decnum = Convert.ToInt32(lineArray[i], 16);//string转化为int
                }
                bytebuffer[byteCount] = Convert.ToByte(decnum);//int转化为Byte
                byteCount++;
            }
            return bytebuffer;
        }

        // 字节数组转16进制字符串
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }

        static void Main(string[] args)
        {
            SqlConnection Conn = new SqlConnection("Data Source=bds27116147.my3w.com;Integrated Security=False;User ID=bds27116147;Password=Sgdpz1234;");
          
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            if (ports.Length != 0)
            {
                string portname = ports[0];
                Console.WriteLine("当前存在端口"+portname);

                using (SerialPort serialPort1 = new SerialPort())
                {
                    serialPort1.PortName = portname;//设置COM口
                    serialPort1.BaudRate = 38400;//设置波特率
                    serialPort1.DataBits = 8;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Parity = Parity.None;


                    string order = "01 03 00 00 00 02 C4 0B";
                    byte[] Odrer = StringToHex(order);
                    Byte[] RecData = new Byte[9];

                    serialPort1.Open();//打开串口
                    //serialPort1.DtrEnable = true;
         
                    Conn.Open();
                    SqlCommand cmd = Conn.CreateCommand();
                
                    cmd.CommandText = "SELECT TOP 1 * FROM [bds27116147_db].[dbo].[weiyilyy] order by [xuhao] desc";
                    SqlDataReader reader = cmd.ExecuteReader();
                    int i = 1;
                    while (reader.Read())
                    {
                        i = reader.GetInt32(reader.GetOrdinal("xuhao"));
                    }
                    Conn.Close();
                    Conn.Open();
                    for (; ; )
                    {
                        serialPort1.Write(Odrer, 0, Odrer.Length);
                        System.Threading.Thread.Sleep(100);//延时100ms等待接收完数据
                        serialPort1.Write(Odrer, 0, Odrer.Length);
                        System.Threading.Thread.Sleep(100);//延时100ms等待接收完数据
                        serialPort1.Write(Odrer, 0, Odrer.Length);
                        System.Threading.Thread.Sleep(100);//延时100ms等待接收完数据
                        serialPort1.Write(Odrer, 0, Odrer.Length);
                        System.Threading.Thread.Sleep(100);//延时100ms等待接收完数据


                        serialPort1.Read(RecData, 0, 9);
                        serialPort1.DiscardInBuffer();
                                        
                        string str = byteToHexStr(RecData);
                        string data = str.Substring(10, 4);
                        int a = Int32.Parse(data, System.Globalization.NumberStyles.HexNumber);
                        string aa = a.ToString();

                        i++;
                        cmd.CommandText = "INSERT INTO [bds27116147_db].[dbo].[weiyilyy](xuhao,weiyi,timenow) VALUES('" + i + "','" + a + "',GETDATE())";
                        cmd.ExecuteNonQuery();
                     
                        Console.WriteLine(aa);
                        System.Threading.Thread.Sleep(1000);//延时1000ms等待接收完数据
                    }
                    Conn.Close();
                }
            }
            else
            {
                Console.WriteLine("当前不存在串口");
            }
            Console.ReadKey();
        }
    }
}
