using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static Nireus.Delegate;

namespace Nireus
{
	public class SocketService
	{
		enum SocketState
		{
			DISCONNECT,		// 断线;
            SERVER_HALT,     //服务器断线;
            CONNECTING,		// 连接中;
			CONNECTED		// 连接上;
		};
        private System.Object statusLock = new System.Object();
        private System.Object socketLock = new System.Object();
        //static private SocketService _instance;
		//static public SocketService getInstance() { return _instance == null ? _instance = new SocketService() : _instance; }

		private const Byte SOCKET_RESULT_FAIL = 0;
		private const Byte SOCKET_RESULT_SUCC = 1;

		private string _ip;
        public string ip { get { return _ip; } }
		private int _port;
        public int port { get { return _port; } }

        private IPEndPoint _ipEndPoint = null;
        private Socket _socket = null;
        private SocketAsyncEventArgs _socketAsyncEventArgs;
        private SocketState _socket_state;
		private SocketState _pre_socket_state;
        private bool _first_connect;
        protected bool _need_reconnect = false;

        private Queue<NetData> _recive_datas = new Queue<NetData>();
		private Deque<NetData> _send_datas = new Deque<NetData>();

        // 回调;
        //public Delegate.CallFuncVoid _on_start_rpc_callback = null;
        //public Delegate.CallFuncVoid _on_end_rpc_callback = null;
        private Delegate.CallFuncVoid _on_connect_succ_callback = null;
		private Delegate.CallFuncVoid _on_connect_fail_callback = null;
        private Delegate.CallFuncVoid _on_server_halt_callback = null;
        private Delegate.CallFuncVoid _on_close_callback = null;
        private Delegate.CallFuncNetData _on_call_failed_callback = null;

        private Dictionary<int, Delegate.CallFuncNetData> _reply_succ_callback_map = new Dictionary<int, Delegate.CallFuncNetData>();
        private Dictionary<int, Delegate.CallFuncNetData> _reply_fail_callback_map = new Dictionary<int, Delegate.CallFuncNetData>();
		private Dictionary<int, List<Delegate.CallFuncNetData>> _notify_callback_map = new Dictionary<int, List<Delegate.CallFuncNetData>>();

	    private Delegate.CallFuncNetData _reply_fail_default_callback;

        public SocketService()
		{
			_pre_socket_state = _socket_state = SocketState.DISCONNECT;
		}

		public void setTarget(string ip, int port)
		{
            if(ip != _ip)
            {
                _ip = ip;
                _ipEndPoint = null;
            }
            if (port != _port)
            {
                _port = port;
                _ipEndPoint = null;
            }
		}

		public void setOnConnectSucceCallback(Delegate.CallFuncVoid callback) { _on_connect_succ_callback = callback; }
		public void setOnConnectFailCallback(Delegate.CallFuncVoid callback) { _on_connect_fail_callback = callback; }
		public void setOnCloseCallback(Delegate.CallFuncVoid callback) { _on_close_callback = callback; }
        public void setOnServerHaltCallBack(Delegate.CallFuncVoid callback) { _on_server_halt_callback = callback; }
		public void setCallFailback(Delegate.CallFuncNetData callback) { _on_call_failed_callback = callback; }

	    public void setReplyFailDefaultCallback(Delegate.CallFuncNetData callback) { _reply_fail_default_callback = callback; }

        private void setSocketState(SocketState socket_state)
		{
            lock (statusLock)
            {
                _pre_socket_state = _socket_state;
                _socket_state = socket_state;
            }
		}

        private float _last_link_time;
		public void doConnect()
        {
            float t = Time.realtimeSinceStartup;
            if (t - _last_link_time < 1) return;
            _last_link_time = t;
            //GameDebug.LogError("socket ==> doConnect");
            try
			{
                lock (statusLock)
                {
                    if (_socket_state == SocketState.CONNECTING) return;
                }
                setSocketState(SocketState.CONNECTING);
                if (_socket == null)
                {
                    //GameDebug.LogError("socket ==> doConnect socket == null");
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                if(_socketAsyncEventArgs == null)
                {
                    _socketAsyncEventArgs = new SocketAsyncEventArgs();
                    _socketAsyncEventArgs.UserToken = _socket;
                    _socketAsyncEventArgs.RemoteEndPoint = _ipEndPoint;
                    _socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnectAsyncCompleted);
                }
                if(_ipEndPoint == null)
                {
                    //GameDebug.LogError("socket ==> doConnect ipEndPoint == null");
                    _ipEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
                    _socketAsyncEventArgs.RemoteEndPoint = _ipEndPoint;
                }
                GameDebug.LogErrorFormat("socket ==> doConnect Connected={0}", _socket.Connected);
                if(_socket.ConnectAsync(_socketAsyncEventArgs) == false)
                {
                    setSocketState(SocketState.DISCONNECT);
                    GameDebug.Log("SocketService::ConnectAsync false exception: SocketError => " + _socketAsyncEventArgs.SocketError + ", ConnectByNameError => " + _socketAsyncEventArgs.ConnectByNameError);
                }
            }
			catch (System.Exception ex)
			{
				setSocketState(SocketState.DISCONNECT);
				GameDebug.Log("SocketService::doConnect exception: " + ex.Message);
                //if (_on_end_rpc_callback != null)
                //    _on_end_rpc_callback();
            }
		}

        private void OnConnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            if(_socketAsyncEventArgs == null 
                || _socketAsyncEventArgs.SocketError != SocketError.Success
                || _socket == null || _socket.Connected == false)
            {
                setSocketState(SocketState.DISCONNECT);
                GameDebug.Log("SocketService::OnConnectAsyncCompleted: SocketError => " + _socketAsyncEventArgs.SocketError + ", ConnectByNameError => " + _socketAsyncEventArgs.ConnectByNameError);
                return;
            }
            try
            {
                lock (statusLock)
                {
                    _pre_socket_state = _socket_state;
                    _socket_state = SocketState.CONNECTED;
                    GameDebug.Log("OnConnectAsyncCompleted connect to ip: " + _ip + ":" + _port + " success! " + "_pre_socket_state: " + _pre_socket_state
                        + " _socket_state: " + _socket_state);
                }
                NetData recv_data = NetData.pop();
                _socket.BeginReceive(recv_data.getBuffer(), 0, NetData.HEAD_LENGTH, SocketFlags.None, receiveHeader, recv_data);
                //_socket.BeginConnect(IPAddress.Parse(_ip), _port, new System.AsyncCallback(connectCallback), _socket);
            }
            catch (System.Exception ex)
            {
                setSocketState(SocketState.DISCONNECT);
                GameDebug.Log("SocketService::BeginReceive exception: " + ex.Message);
                //if (_on_end_rpc_callback != null)
                //    _on_end_rpc_callback();
            }
        }

        public bool Connected { get { return _socket != null && _socket.Connected; } }

        public void disconnect()
        {
            lock (socketLock)
            {
                if (_socket == null)
                {
                    //GameDebug.LogError("socket ==> disconnect socket=null");
                    return;
                }
                //Debug.LogErrorFormat("socket ==> disconnect Connected={0}", _socket.Connected);
                try
                {
                    if(_socket.Connected)   _socket.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception e)
                {
                    GameDebug.LogError(e.ToString());
                }
                try
                {
                    //GameDebug.LogError("socket ==> disconnect socket.Close");
                    _socket.Close();
                }
                catch (Exception e)
                {

                    GameDebug.LogError(e.ToString());
                }
                //GameDebug.LogError("socket ==> disconnect socket.Close end");
                _socket = null;
                GameDebug.Log("Socket disconnect!");
            }
            setSocketState(SocketState.DISCONNECT);
        }

        private void _onServerHalt()
        {
            lock (socketLock)
            {
                if (_socket == null)
                {
                    return;
                }
                try
                {
                    if (_socket.Connected) _socket.Shutdown(SocketShutdown.Receive);
                }
                catch (Exception e)
                {
                    GameDebug.LogError(e.ToString());
                }
                try
                {
                    _socket.Close();
                }
                catch (Exception e)
                {

                    GameDebug.LogError(e.ToString());
                }
                _socket = null;
                GameDebug.Log("Socket disconnect!");
            }
            setSocketState(SocketState.SERVER_HALT);
        }


        public virtual void Update()
		{
            CallFuncVoid call_back = null;

            lock (statusLock)
            {
                if (_socket_state == SocketState.DISCONNECT && _pre_socket_state == SocketState.DISCONNECT )
                {
                    if(_need_reconnect)//需要重新连接
                    {
                        doConnect();
                    }
                    return;
                }

                if(_socket_state == SocketState.CONNECTING && _pre_socket_state == SocketState.DISCONNECT)
                {
                    if(_socketAsyncEventArgs != null && _socketAsyncEventArgs.SocketError == SocketError.Success)
                    {
                        if(_socket != null && _socket.Connected)
                        {
                            OnConnectAsyncCompleted(null, _socketAsyncEventArgs);
                        }
                        else
                        {
                            setSocketState(SocketState.DISCONNECT);
                            GameDebug.Log("SocketService::Update set SocketState.DISCONNECT");
                        }
                    }
                }
                if (_socket_state == SocketState.CONNECTED && _pre_socket_state == SocketState.CONNECTING)
                {
                    //if (_first_connect)
                    //{
                    //    // 第一次连接成功;
                    _on_connect_succ_callback?.Invoke();
                    //    _first_connect = false;
                    //}
                    //else
                    //{
                    //    //Game.Global globa = GameObject.Find("Game Global").GetComponent<Game.Global>();
                    //    //globa.systemLogin();
                    //}
                    _pre_socket_state = _socket_state;

                    this.OnConnect();

                }

                if(_socket_state == SocketState.SERVER_HALT)
                {
                    _on_server_halt_callback?.Invoke();
                    _pre_socket_state = _socket_state;
                    _socket_state = SocketState.DISCONNECT;
                }


                if (_socket_state == SocketState.DISCONNECT && _pre_socket_state == SocketState.CONNECTING)
                {
                    // 连接失败;
                    call_back = _on_connect_fail_callback;
                    _pre_socket_state = _socket_state;
                    //if (_on_end_rpc_callback != null)
                    //    _on_end_rpc_callback();
                }

                if (_socket_state == SocketState.DISCONNECT && _pre_socket_state == SocketState.CONNECTED)
                {
                    // 断开连接;
                    call_back = _on_close_callback;
                    _pre_socket_state = _socket_state;
                    //if (_on_end_rpc_callback != null)
                    //    _on_end_rpc_callback();
                }

                // 发送;
                if (_socket_state == SocketState.CONNECTED)
                {
                    NetData data = peekSendData();
                    if (data != null)
                    {
                        send(data);
                    }
                }

                // 接收;
                {
                    NetData data = peekReciveData();
                    if (data != null)
                    {
                        onProcessReciveData(data);
                        //if (_on_end_rpc_callback != null)
                        //    _on_end_rpc_callback();
                    }
                }
            }


            call_back?.Invoke();

        }

		public void registerNotify(int proc, Delegate.CallFuncNetData callback)
		{
            List<Delegate.CallFuncNetData> list;
            if (_notify_callback_map.TryGetValue(proc, out list) == false)
            {
                list = new List<Delegate.CallFuncNetData>();
                _notify_callback_map.Add(proc, list);
            }
            if (list.Contains(callback) == false)
            {
                list.Add(callback);
            }
        }

        public void unregisterNotify(int proc, Delegate.CallFuncNetData callback)
        {
            List<Delegate.CallFuncNetData> list;
            if (_notify_callback_map.TryGetValue(proc, out list))
            {
                list.Remove(callback);
            }
        }

        private void notify(NetData data)
        {
            //notify to registered proc
            List<Delegate.CallFuncNetData> list;
            int readOffset = data.getReadOffset();
            data.setReadOffset(readOffset);
            if (_notify_callback_map.TryGetValue(data.getProc(), out list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Delegate.CallFuncNetData callback = list[i];
                    callback(data);
                }
            }
        }

        public void callProc(int proc, Delegate.CallFuncNetData succ_reply = null, Delegate.CallFuncNetData fail_reply = null, bool blockUI = false)
		{
			NetData data = NetData.pop(proc);
			callProc(data, succ_reply, fail_reply, blockUI);
		}

		public void callProc(NetData data, Delegate.CallFuncNetData succ_reply = null, Delegate.CallFuncNetData fail_reply = null,bool blockUI = false)
		{
            //if(blockUI && _on_start_rpc_callback != null)
            //    _on_start_rpc_callback();

            checkIsConnected();

			data.setPackType(PackType.PACK_TYPE__PROC);
			if (succ_reply != null) _reply_succ_callback_map.Add(data.getClientOder(), succ_reply);
			if (fail_reply != null) _reply_fail_callback_map.Add(data.getClientOder(), fail_reply);

			pushSendData(data);
		}

		public void callProcDirectly(NetData data, Delegate.CallFuncNetData succ_reply = null, Delegate.CallFuncNetData fail_reply = null)
		{
			checkIsConnected();

			data.setPackType(PackType.PACK_TYPE__PROC);
			if (succ_reply != null) _reply_succ_callback_map.Add(data.getClientOder(), succ_reply);
			if (fail_reply != null) _reply_fail_callback_map.Add(data.getClientOder(), fail_reply);
			pushSendData(data, false);
		}

		public void sendProc(int proc)
		{
			NetData data = NetData.pop(proc);
			sendProc(data);
		}

		public void sendProc(NetData data)
		{
			checkIsConnected();
			data.setPackType(PackType.PACK_TYPE__ASYNCPROC);
			pushSendData(data);
		}

		private void connectCallback(System.IAsyncResult ar)
		{
			try
			{
                _socket.EndConnect(ar);
                lock (statusLock)
                {
                    _pre_socket_state = _socket_state;
                    _socket_state = SocketState.CONNECTED;
                    GameDebug.Log("connect to ip: " + _ip + ":" + _port + " success! " + "_pre_socket_state: " + _pre_socket_state
                        + " _socket_state: " + _socket_state);
                }
                //Thread.Sleep(100);
                NetData recv_data = NetData.pop();
                _socket.BeginReceive(recv_data.getBuffer(), 0, NetData.HEAD_LENGTH, SocketFlags.None, receiveHeader, recv_data);
			}
			catch (System.Exception ex)
			{
				GameDebug.Log("SocketService::connectCallback exception: " + ex.Message);
				if (_socket.Connected)
				{
					disconnect();
				}
				else
				{
					setSocketState(SocketState.DISCONNECT);
				}
			}
		}
        

        private void receiveHeader(System.IAsyncResult ar)
		{
            try
            {
                //Debug.LogErrorFormat("socket ==> receiveHeader Connected={0}", _socket.Connected);
                // if (_socket.Connected == false) return;
                
                    //GameDebug.Log(_socket.Connected + "_socket");
                int read = _socket.EndReceive(ar);

                //Debug.LogErrorFormat("socket ==> receiveHeader EndReceive={0}", read);
                if (read < 1)
                {
                    // 连接关闭;
                    GameDebug.Log("disconnect by server!");
                    _onServerHalt();
                    return;
                }
                
				NetData data = (NetData)ar.AsyncState;
				data.parseHead();

				if (data.getWriteSize() == 0)
				{
					// 加入到接收队列中;
					addReciveData(data);

					// 重新读取header数据;
					data = NetData.pop();
					_socket.BeginReceive(data.getBuffer(), 0, NetData.HEAD_LENGTH, SocketFlags.None, receiveHeader, data);
				}
				else
				{
					// 读取body数据;
					_socket.BeginReceive(data.getBuffer(), NetData.HEAD_LENGTH, data.getWriteSize(), SocketFlags.None, receiveBody, data);
				}
			}
			catch (System.Exception ex)
			{
				GameDebug.Log("SocketService::reciveHeader exception: " + ex.StackTrace);
				disconnect();
			}
		}

		private void receiveBody(System.IAsyncResult ar)
		{
			try
			{
				int read = _socket.EndReceive(ar);
				if (read < 1)
				{
					// 连接关闭;
					GameDebug.Log("disconnect by server!");
					disconnect();
					return;
				}

				NetData data = (NetData)ar.AsyncState;

				data.addCursor(read);
				if (data.getCursor() == data.getWriteSize())
				{
					// 加入到接收队列中;
					addReciveData(data);

					// 重新读取header数据;
					data = NetData.pop();
					_socket.BeginReceive(data.getBuffer(), 0, NetData.HEAD_LENGTH, SocketFlags.None, new System.AsyncCallback(receiveHeader), data);
				}
				else
				{
					// 继续读取包身;
					_socket.BeginReceive(data.getBuffer(), data.getCursor() + NetData.HEAD_LENGTH, data.getWriteSize() - data.getCursor(),
						SocketFlags.None, new System.AsyncCallback(receiveBody), data);
				}
			}
			catch (System.Exception ex)
			{
				GameDebug.Log("SocketService::receiveBody exception: " + ex.Message);
				disconnect();
			}
		}

		protected void send(NetData data)
		{
			try
			{
				data.makeHead();

				_socket.BeginSend(data.getBuffer(), 0, data.getBufferSize(), SocketFlags.None, new System.AsyncCallback(sendCallback), data);
			}
			catch (System.Exception ex)
			{
				GameDebug.Log("SocketService::send exception: " + ex.Message);
				disconnect();
			}
		}

		private void sendCallback(System.IAsyncResult ar)
		{
			try
			{
				int send = _socket.EndSend(ar);
				if (send < 1)
				{
					// 连接关闭;
					disconnect();
					return;
				}

				NetData data = (NetData)ar.AsyncState;
				data.addCursor(send);
				if (data.getCursor() == data.getBufferSize())
				{
					NetData.push(data);
				}
				else
				{
					_socket.BeginSend(data.getBuffer(), data.getCursor(), data.getBufferSize() - data.getCursor(), SocketFlags.None, new System.AsyncCallback(sendCallback), data);
				}
			}
			catch (System.Exception ex)
			{
				GameDebug.Log("SocketService::sendCallback exception: " + ex.Message);
				disconnect();
			}
		}

		private void onReply(NetData data)
		{
            byte result = data.readByte();
			if (result == SOCKET_RESULT_SUCC)
			{
				Delegate.CallFuncNetData callback;
				if (_reply_succ_callback_map.TryGetValue(data.getClientOder(), out callback))
				{
					callback(data);
					_reply_succ_callback_map.Remove(data.getClientOder());
				}
			}
			else if (result == SOCKET_RESULT_FAIL)
			{
				Delegate.CallFuncNetData callback;
				if (_reply_fail_callback_map.TryGetValue(data.getClientOder(), out callback))
				{
					callback(data);
					_reply_fail_callback_map.Remove(data.getClientOder());
				}
				else
				{
                    _reply_fail_default_callback?.Invoke(data);
				}
			}
			else if(!onOtherResultReply(result, data))
			{
				data.resetReadOffset();
				_on_call_failed_callback?.Invoke(data);
			}

			NetData.push(data);
		}

        protected virtual bool onOtherResultReply(byte result, NetData data)
        {
            return false;
        }

        private void onReceive(NetData data)
		{
            notify(data);
        }


		private NetData peekReciveData()
		{
			NetData data = null;
			lock (_recive_datas)
			{
				if (_recive_datas.Count != 0)
				{
					data = _recive_datas.Dequeue();
				}
			}
			return data;
		}
		private bool hasReciveData()
		{
			lock (_recive_datas)
			{
				return _recive_datas.Count > 0;
			}
		}

		private NetData peekSendData()
		{
			lock (_send_datas)
			{
				if (_send_datas.Count > 0)
				{
					return _send_datas.shift();
				}
			}

			return null;
		}

		protected virtual void pushSendData(NetData data, bool is_in_oder = true)
		{
			lock (_send_datas)
			{
				if (is_in_oder) _send_datas.push(data);
				else _send_datas.unshift(data);
			}
		}

		private void checkIsConnected()
		{
            lock (statusLock)
            {
                if (_socket_state == SocketState.DISCONNECT)
                {
                    doConnect();
                }
            }
		}

        protected virtual void addReciveData(NetData data)
		{
			lock (_recive_datas)
			{
				_recive_datas.Enqueue(data);
			}
		}
        //public void setFirstConnect(bool first_connect)
        //{
        //    _first_connect = first_connect;
        //}

        protected void onProcessReciveData(NetData data)
        {
            if (data.getPackType() == PackType.PACK_TYPE__PROCREPLY)
            {
                onReply(data);
            }
            else if (data.getPackType() == PackType.PACK_TYPE__DISPATCH)
            {
                onReceive(data);
            }
        }
        protected virtual void OnConnect()
        {
            String str = "Nireus00";
            Byte[] b = new Byte[str.Length];
            System.Text.Encoding.ASCII.GetBytes(str, 0, str.Length, b, 0);
            lock (socketLock)
            {
                if (_socket == null)
                {
                    //GameDebug.LogError("socket ==> OnConnect socket == null");
                    GameDebug.LogError("OnConnect socket == null");
                }
                else
                {
                    //Debug.LogErrorFormat("socket ==> OnConnect Connected={0}", _socket.Connected);
                    _socket.Send(b);
                }
            }
        }
    }
}