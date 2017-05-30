using System;
using System.Text;

namespace FileDB
{
    public partial class FileDBMS
    {
        private byte[] FromString(string s){
            return Encoding.ASCII.GetBytes(s);
        }

        private string ToString(byte[] bytes){
            if(bytes.Length == 0)
                return "";
            return Encoding.ASCII.GetString(bytes);
        }

        private byte[] FromInt(int s){
            return BitConverter.GetBytes(s);
        }

        private int ToInt(byte[] bytes){
            if(bytes.Length == 0)
                return -1;
            return BitConverter.ToInt32(bytes, 0);
        }

        private byte[] FromDouble(double s){
            return BitConverter.GetBytes(s);
        }

        private double ToDouble(byte[] bytes){
            if(bytes.Length == 0)
                return -1;
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}