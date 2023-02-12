using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
	public enum PackType
	{
		PACK_TYPE__PROC = 1,
		PACK_TYPE__PROCREPLY = 2,
		PACK_TYPE__DISPATCH = 3,
		PACK_TYPE__ASYNCPROC = 4,
		PACK_TYPE__MEM = 5,
		PACK_TYPE__ECHO = 0xff
	}

	public enum ProcType
	{
		PROC_TYPE__USER = 0,
		PROC_TYPE__SERVER_VERIFY = 1,
		PROC_TYPE__SERVER_NORMAL = 2,
		PROC_TYPE__SEND = 3,
		PROC_TYPE__CALL = 4
	}

	public enum NireusProcDef
	{
		NIREUS_USER_PROC = 100,
		NIREUS_USER_NOTIFY = 102,
		NIREUS_USER_REMOTE_LOGIN = 103,
		NIREUS_USER_REMOTE_LOGOUT = 104
	}

	public class NetData : ICloneable, IByteReader
	{
		public const int HEAD_LENGTH = 18;
		public const int NET_DATA_USER_BLOCK_MAX = 4;
		public const int NET_DATA_SERVER_BLOCK_MAX = 1024;

		static private int _global_unique_order = 0;

		static private Queue<NetData> _net_data_pool = new Queue<NetData>();
		static public NetData pop(int proc = 0)
		{
			NetData data = null;
			lock (_net_data_pool)
			{
				if (_net_data_pool.Count != 0)
				{
					data = _net_data_pool.Dequeue();
					data.initialize();
				}
				else
				{
					data = new NetData();
				}
				data.setProc(proc);
			}

			return data;
		}

		static public void push(NetData data)
		{
			lock (_net_data_pool)
			{
				_net_data_pool.Enqueue(data);
			}
		}

		public NetData(int size = 2048)
		{
			_block_size = size;
			_block_count = 1;
			_buffer = new byte[_block_size * _block_count];

			initialize();
		}

		public void initialize()
		{
			_valid_size = _block_size * _block_count - HEAD_LENGTH;
			_data_size = 0;
			_write_size = 0;
			_read_offset = 0;
			_cursor = 0;

			_proc = 0;
			_proc_type = 0;
			_pack_type = 0;
			_client_oder = 0;
			_user_pos = 0;
		}

		public void writeBool(bool c)
		{
			writeByte(c ? (Byte)1 : (Byte)0);
		}

        public void writeInt8(byte c)
        {
            writeByte(c);
        }

        public void writeByte(byte c)
		{
			int length = sizeof(byte);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}

			byte[] bs = System.BitConverter.GetBytes(c);
			bs.CopyTo(_buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
        }
        public void writeInt16(short c)
        {
            writeShort(c);
        }

        public void writeShort(short c)
		{
			int length = sizeof(short);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}

			byte[] bs = System.BitConverter.GetBytes(c);
			bs.Reverse();
			bs.CopyTo(_buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}

        public void writeInt32(int c)
        {
            writeInt(c);
        }
        private void writeInt(int c)
		{
			int length = sizeof(int);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}

			byte[] bs = System.BitConverter.GetBytes(c);
			bs.CopyTo(_buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}

		public void writeInt64(Int64 c)
		{
			writeString(c.ToString());
		}

		public void writeFloat(float c)
		{
			int length = sizeof(float);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}

			byte[] bs = System.BitConverter.GetBytes(c);
			bs.CopyTo(_buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}

        internal static NetData pop(object wORLD_PROC_REQUEST_USER_CITY_UI_INFO)
        {
            throw new NotImplementedException();
        }

        public void writeDouble(double c)
		{
			int length = sizeof(double);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}

			byte[] bs = System.BitConverter.GetBytes(c);
			bs.CopyTo(_buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}

		public void writeString(string str)
		{
			short length = (short)System.Text.Encoding.UTF8.GetByteCount(str);
            writeShort(length);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}
			System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, _buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}
        
        public void writeBytes(byte[] bytes, int index, int length)
        {
            int new_size = _data_size + length;
            if (new_size > _valid_size && !externBuf(new_size))
            {
                throw new System.OverflowException("out of range");
            }
            Array.Copy(bytes, index, _buffer, getBufferSize(), length);
            _data_size += length;
        }

        public void writeBytes(IByteReader bytes, int length = 0)
        {
            if (length == 0) length = bytes.getDataSize() - bytes.getReadOffset();
            int new_size = _data_size + length;
            if (new_size > _valid_size && !externBuf(new_size))
            {
                throw new System.OverflowException("out of range");
            }
            bytes.readBytes(_buffer, HEAD_LENGTH + _data_size, length);
            _data_size += length;
        }

        public void writeASCIIString(string str)
		{
			short length = (short)System.Text.Encoding.ASCII.GetByteCount(str);
			writeShort(length);

			int new_size = _data_size + length;
			if (new_size > _valid_size && !externBuf(new_size))
			{
				throw new System.OverflowException("out of range");
			}
			System.Text.Encoding.ASCII.GetBytes(str, 0, str.Length, _buffer, HEAD_LENGTH + _data_size);
			_data_size += length;
		}



		public bool readBool()
		{
			return readByte() > 0;
		}

		public byte readByte()
		{
			int length = sizeof(Byte);

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			Byte ret = _buffer[HEAD_LENGTH + _read_offset];
			_read_offset += length;
			return ret;
        }

        public short readInt16()
        {
            return readShort();
        }

        public short readShort()
		{
			int length = sizeof(short);

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			short ret = System.BitConverter.ToInt16(_buffer, HEAD_LENGTH + _read_offset);
			_read_offset += length;
			return ret;
		}
        public byte readInt8()
        {
            return readByte();
        }

        public int readInt32()
        {
            return readInt();
        }

        public int readInt()
		{
			int length = sizeof(int);

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			int ret = System.BitConverter.ToInt32(_buffer, HEAD_LENGTH + _read_offset);
			_read_offset += length;
			return ret;
		}

		public long readInt64()
		{
			string s = readString();
			return Convert.ToInt64(s);
		}
        public float readFloat()
		{
			int length = sizeof(float);

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			float ret = System.BitConverter.ToSingle(_buffer, HEAD_LENGTH + _read_offset);
			_read_offset += length;
			return ret;
		}

		public double readDouble()
		{
			int length = sizeof(double);

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			double ret = System.BitConverter.ToDouble(_buffer, HEAD_LENGTH + _read_offset);
			_read_offset += length;
			return ret;
		}

		public string readASCIIString()
		{
			short length = readShort();

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			string str = Encoding.ASCII.GetString(_buffer, HEAD_LENGTH + _read_offset, length);
			_read_offset += length;
			return str;
		}

		public string readString()
		{
			short length = readShort();

			if (_read_offset + length > _valid_size)
			{
				throw new System.OverflowException("out of range");
			}

			string str = Encoding.UTF8.GetString(_buffer, HEAD_LENGTH + _read_offset, length);
			_read_offset += length;
			return str;
		}


		// 重新申请内存块;
		private bool externBuf(int new_size)
		{
			int new_block_count = (new_size + HEAD_LENGTH) / _block_size;
			if ((new_size + HEAD_LENGTH) % _block_size != 0) ++new_block_count;

			if ((_pack_type == PackType.PACK_TYPE__PROC || _pack_type == PackType.PACK_TYPE__ASYNCPROC) && _proc_type == ProcType.PROC_TYPE__USER)
			{
				if (new_block_count >= NET_DATA_USER_BLOCK_MAX) return false;
			}
			else if (_pack_type != PackType.PACK_TYPE__MEM && _pack_type != PackType.PACK_TYPE__DISPATCH)
			{
				if (new_block_count >= NET_DATA_SERVER_BLOCK_MAX) return false;
			}

			if (new_block_count <= _block_count) return true;
			if (new_block_count < _block_count * 2)
			{
				new_block_count = _block_count * 2;
			}

			Byte[] new_buffer = new Byte[new_block_count * _block_size];
			_buffer.CopyTo(new_buffer, 0);
			_buffer = new_buffer;
			_block_count = new_block_count;
			_valid_size = _block_count * _block_size - HEAD_LENGTH;

			return true;
		}

		public void setWriteSize(int size)
		{
			_write_size = size;
			if (_write_size > _valid_size && !externBuf(_write_size))
			{
				throw new System.OverflowException("out of range");
			}
		}

        public void setReadOffset(int size)
        {
            _read_offset = size;
            if (_read_offset > _valid_size)
            {
                throw new System.OverflowException("out of range");
            }
        }

        public void parseHead()
		{
			setPackType((PackType)_buffer[0]);
			setProcType((ProcType)_buffer[1]);
			setProc(System.BitConverter.ToInt32(_buffer, 2));
			setClientOder(System.BitConverter.ToInt32(_buffer, 6));
			setUserPos(System.BitConverter.ToInt32(_buffer, 10));
			setWriteSize(System.BitConverter.ToInt32(_buffer, 14));
            _data_size = _write_size;
        }

		public void makeHead()
		{
			System.BitConverter.GetBytes((Byte)getPackType()).CopyTo(_buffer, 0);
			System.BitConverter.GetBytes((Byte)getProcType()).CopyTo(_buffer, 1);
			System.BitConverter.GetBytes((int)getProc()).CopyTo(_buffer, 2);
			System.BitConverter.GetBytes((int)getClientOder()).CopyTo(_buffer, 6);
			System.BitConverter.GetBytes((int)getUserPos()).CopyTo(_buffer, 10);
			System.BitConverter.GetBytes((int)getDataSize()).CopyTo(_buffer, 14);
		}
        public void setBuffer(byte[] buffer)
        {
            _buffer = buffer;
            _data_size = buffer.Length - HEAD_LENGTH;
        }

        public Byte[] getBuffer() { return _buffer; }
		public Byte[] getData() { return _buffer; }

		public int getProc() { return _proc; }
		public ProcType getProcType() { return (ProcType)_proc_type; }
		public PackType getPackType() { return (PackType)_pack_type; }
		public int getUserPos() { return _user_pos; }
		public int getClientOder()
		{
			if (_client_oder != 0) return _client_oder;
			_client_oder = ++_global_unique_order;
			return _client_oder;
		}

		public int getDataSize() { return _data_size; }
		public int getBufferSize() { return _data_size + HEAD_LENGTH; }
		public int getValidSize() { return _valid_size; }
		public int getWriteSize() { return _write_size; }
		public int getCursor() { return _cursor; }
		public void addCursor(int offset) { _cursor += offset; }

		public void resetReadOffset() { _read_offset = 0; }
        public int getReadOffset() { return _read_offset; }

        public void setProc(int proc) { _proc = proc; }
		public void setProcType(ProcType proc_type) { _proc_type = proc_type; }
		public void setPackType(PackType pack_type) { _pack_type = pack_type; }
		public void setClientOder(int client_oder) { _client_oder = client_oder; }
		public void setUserPos(int user_pos) { _user_pos = user_pos; }

        public object Clone()
        {
            NetData nd = new NetData(_block_size * _block_count);
            Array.Copy(this._buffer, nd._buffer, this._buffer.Length);
            nd._valid_size = _valid_size;
            nd._data_size = _data_size;
            nd._read_offset = _read_offset;
            nd._write_size = _write_size;
            nd._block_size = _block_size;
            nd._block_count = _block_count;
            nd._proc = _proc;
            nd._pack_type = _pack_type;
            nd._proc_type = _proc_type;
            nd._client_oder = _client_oder;
            nd._cursor = _cursor;
            nd._user_pos = _user_pos;
            return nd;
        }

        public float readFloat32()
        {
            return readFloat();
        }

        public double readFloat64()
        {
            return readDouble();
        }

        public void changeLastByte(bool c)
        {
            _buffer[_buffer.Length - 1] = c ? (Byte)1 : (Byte)0;
        }

        public void readBytes(byte[] bytes, int offset = 0, int length = 0)
        {
            if (length == 0) length = getDataSize() - getReadOffset();
            Array.Copy(_buffer, HEAD_LENGTH + _read_offset, bytes, offset, length);
            _read_offset += length;
        }
        private byte[] _buffer = null;	// 数据流;

		private int _valid_size;	// 有效长度，即byte数组长度;
		private int _data_size;		// 手动写入包的大小，不包括head;
		private int _read_offset;	// pop时的位移;
		private int _write_size;	// 读取时指定读取大小;

		private int _block_size;	// 内存块大小;
		private int _block_count;	// 内存块数量;

		private int _proc;			// 协议号;
		private PackType _pack_type;// 包类型;
		private ProcType _proc_type;// 命令类型;
		private int _client_oder;	// 客户端序号;
		private int _cursor;		// 数据读入时的位置标记;
		private int _user_pos;		// 内网传输时需要的玩家位置;
	}
}