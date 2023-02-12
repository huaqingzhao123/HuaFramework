using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nireus
{
	public class DataRow
	{
		private List<string> _fields;
		private List<string> _values;

		public void setFields(List<string> fields) { _fields = fields; }
		public void setValues(List<string> values) { _values = values; }
        public bool getBool(int index) { return getString(index) != "0"; }
        public byte getInt8(int index) { return byte.Parse(getString(index)); }
        public short getInt16(int index) { return short.Parse(getString(index)); }
        public Int32 getInt32(int index) { return Int32.Parse(getString(index)); }
		public Int64 getInt64(int index) { return Int64.Parse(getString(index)); }
        public float getFloat32(int index) { return float.Parse(getString(index)); }
	    public Double getFloat64(int index) { return Double.Parse(getString(index)); }
        public Double getDouble(int index) { return Double.Parse(getString(index)); }
		public string getString(int index) { return _values[index]; }
        public bool getBool(string field) { return getString(field) != "0"; }

        public byte getInt8(string field) { return byte.Parse(getString(field)); }
        public short getInt16(string field) { return short.Parse(getString(field)); }
        public Int32 getInt32(string field)	{return Int32.Parse(getString(field));}
		public Int64 getInt64(string field) { return Int64.Parse(getString(field)); }
        public float getFloat32(string field) { return float.Parse(getString(field)); }
        public Double getFloat64(string field) { return Double.Parse(getString(field)); }

        public Int32[] getInt32Array(string field)
        {
            string[] ss = getStringArray(field);
            Int32[] result = new Int32[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt32(ss[i]);
            }
            return result;
        }
        public Int32[] getInt32Array(int index, string delims = "|")
        {
            string[] ss = getStringArray(index, delims);
            Int32[] result = new Int32[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt32(ss[i]);
            }
            return result;
        }
        public short[] getInt16Array(string field)
        {
            string[] ss = getStringArray(field);
            short[] result = new short[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToInt16(ss[i]);
            }
            return result;
        }

        public byte[] getInt8Array(string field)
        {
            string[] ss = getStringArray(field);
            byte[] result = new byte[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToByte(ss[i]);
            }
            return result;
        }

	    public float[] getFloat32Array(int index, string delims = "|")
	    {
	        string[] ss = getStringArray(index, delims);
	        float[] result = new float[ss.Length];
	        for (int i = 0; i < ss.Length; i++)
	        {
	            result[i] = Convert.ToSingle(ss[i]);
	        }
	        return result;
	    }

        public float[] getFloat32Array(string field)
        {
            string[] ss = getStringArray(field);
            float[] result = new float[ss.Length];
            for (int i = 0; i < ss.Length; i++)
            {
                result[i] = Convert.ToSingle(ss[i]);
            }
            return result;
        }

        public string getString(string field)
		{
			int index = _fields.IndexOf(field);
			if (index == -1)
			{
				GameDebug.Log("can not find filed: " + field);
				return "";
			}

            //替换换行符
            var value = _values[index].Replace(@"\n", "\n");
		    value = value.Replace(@"<br>", "\n");
		    
		    value = value.Replace(@"<rn>", "\r\n");
		    value = value.Replace(@"<rr>", "\r");

            return value;
		}
        public string getKeyString(string field)
        {
            int index = _fields.IndexOf(field);
            if (index == -1)
            {
                GameDebug.Log("can not find filed: " + field);
                return "";
            }

            //替换换行符
            var value = _values[index];

            return value;
        }
        public string[] getStringArray(string field, string delims = "|")
		{
			return getString(field).Split(new string[]{delims}, StringSplitOptions.RemoveEmptyEntries);
		}
        public string[] getStringArray(int index, string delims = "|")
        {
            return getString(index).Split(new string[] { delims }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
