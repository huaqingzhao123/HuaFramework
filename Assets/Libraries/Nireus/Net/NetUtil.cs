using System;

namespace Nireus
{
    public static class NetUtil
    {
    /*
        public static Byte[] parseToBytes(Google.Protobuf.IMessage message)
        {
            Int32 data_size = message.CalculateSize();

            byte[] buffer = new byte[data_size];

            Google.Protobuf.CodedOutputStream stream = new Google.Protobuf.CodedOutputStream(buffer);

            message.WriteTo(stream);

            return buffer;
        }

        public static void parseFromBytes( Byte[] bytes, Google.Protobuf.IMessage message)
        {
            message.MergeFrom(new Google.Protobuf.CodedInputStream(bytes));
        }

        public static void parseFromHttpStr(String http_str, Google.Protobuf.IMessage message)
        {
            byte[] binary_data = Convert.FromBase64String(http_str);
            //byte[] binary_data = System.Text.Encoding.GetEncoding("iso8859-1").GetBytes(http_str);
            message.MergeFrom(new Google.Protobuf.CodedInputStream(binary_data));
        }

        public static String parseToHttpStr(Google.Protobuf.IMessage message)
        {
            var buffer = parseToBytes(message);
            //return System.Text.Encoding.GetEncoding("iso8859-1").GetString(buffer);
            return Convert.ToBase64String(buffer);
        }
        */
    }

}
