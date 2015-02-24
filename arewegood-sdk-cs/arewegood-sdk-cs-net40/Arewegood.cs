﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using Zeroconf;
using System.Security;
using System.Threading.Tasks;

namespace arewegood
{
    public class Arewegood
    {
        private SecureString _apiKey;
        private string _serviceName;
        private WebSocket _ws;
        private bool _wsIsAuthenticated;
        private Queue<string> _buffer;

        public event SocketUpHandler SocketUp;
        public delegate void SocketUpHandler();

        public Arewegood(string apiKey, string serviceName = "_awgproxy._tcp.local.")
        {
            
            _serviceName = serviceName;
            _wsIsAuthenticated = false;
            _buffer = new Queue<string>();

            // resolve the service and test/find the first valid websocket
            // authenticate it, to

            
            InitializeApiKey(apiKey);
            

            ZeroconfResolver.ResolveAsync(serviceName).ContinueWith(async (continuation) =>
            {
                var list = await continuation;
                foreach (var host in list)
                {
                    // try to find a valid ws @ a given service
                    var possible = "ws://" + host.IPAddress.ToString() + ":" + host.Services[serviceName].Port;
                    WebSocket socket = new WebSocket(possible);
                    int flag = 0;
                    var proceed = new Task<int>(() => { while (flag == 0) { } return flag; });
                    proceed.Start();
                    socket.Open();
                    socket.Error += (s, e) => { flag = 1; };
                    socket.Opened += (s, a) => { flag = 2; };
                    proceed.Wait();
                    var result = proceed.Result;
                    if (result == 2)
                    {
                        _ws = socket;
                        break;
                    }
                    else
                    {
                        socket.Close();
                        socket = null;
                    }
                }
            }).ContinueWith((continuation) =>
            {
                // if we did, authenticate it using our auth protocol
                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    int flag = 0;
                    var proceed = new Task(() => { while (flag == 0) { } });
                    proceed.Start();
                    string data = null;
                    _ws.MessageReceived += (s, d) =>
                    {
                        data = d.Message; flag = d.Message.Length;
                    };
                    _ws.Send(new ApiAuthenticationObject(_apiKey.ToString()).ToString());
                    proceed.Wait();
                    if (data != null && data.Length > 0)
                    {
                        var response = JObject.Parse(data);
                        if (response["type"].ToString() == "api_token-response" &&
                            response["data"].ToString() == "OK")
                        {
                            // ok, we're authenticated!
                            _wsIsAuthenticated = true;
                        }
                    }
                }
            }).ContinueWith((continuation) =>
            {

                SocketUp.Invoke();
                // write any buffered messages
                if (_ws != null && _ws.State == WebSocketState.Open && _wsIsAuthenticated && _buffer.Count > 0)
                {
                    foreach (var b in _buffer)
                        _ws.Send(b);
                }
            });
        }

       unsafe public void InitializeApiKey(string apiKey)
       {
           fixed (char* pChars = apiKey.ToCharArray())
           {
               _apiKey = new SecureString(pChars, apiKey.ToCharArray().Length);

           }
        }

        private void InternalLogAsync(string type, params object[] objs)
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                _ws.Send(new ApiExceptionObject(type, objs).ToString());
            }
            else
            {
                _buffer.Enqueue(new ApiExceptionObject(type, objs).ToString());
            }
        }


        private void InternalLogSync(string type, params object[] objs)
        {
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                int flag = 0;
                var proceed = new Task(() => { while (flag == 0) { } });
                proceed.Start();
                this.SocketUp += () => { flag = 1; };
                proceed.Wait();
            }
            _ws.Send(new ApiExceptionObject(type, objs).ToString());
        }

        public void TraceAsync(params object[] objs)
        {
            InternalLogAsync("trace", objs);
        }

        public void Trace(params object[] objs)
        {
            InternalLogSync("trace", objs);
        }

        public void DebugAsync(params object[] objs)
        {
            InternalLogAsync("debug", objs);
        }

        public void Debug(params object[] objs)
        {
            InternalLogSync("debug", objs);
        }

        public void Info(params object[] objs)
        {
            InternalLogSync("info", objs);
        }

        public void InfoAsync(params object[] objs)
        {
            InternalLogAsync("info", objs);
        }

        public void Error(params object[] objs)
        {
            InternalLogSync("error", objs);
        }

        public void ErrorAsync(params object[] objs)
        {
            InternalLogAsync("error", objs);
        }

    }
}
