using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using Zeroconf;
using System.Threading.Tasks;

namespace arewegood
{
    public class Arewegood
    {
        private string _apiKey;
        private string _serviceName;
        private WebSocket _ws;
        private bool _wsIsAuthenticated;
        private Queue<string> _buffer;

        public Arewegood(string apiKey, string serviceName = "arewegood-proxy")
        {
            _apiKey = apiKey;
            _serviceName = serviceName;
            _wsIsAuthenticated = false;
            _buffer = new Queue<string>();

            // resolve the service and test/find the first valid websocket
            // authenticate it, to
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
            }).ContinueWith((continuation) => {
                // if we did, authenticate it using our auth protocol
                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    int flag = 0;
                    var proceed = new Task(() => { while (flag == 0) { } });
                    proceed.Start();
                    byte[] data = null;
                    _ws.DataReceived += (s, d) => { data = d.Data; };
                    _ws.Send(new ApiAuthenticationObject(_apiKey).ToString());
                    proceed.Wait();
                    if (data != null && data.Length > 0)
                    {
                        var jsonString = System.Text.UTF8Encoding.UTF8.GetString(data);
                        var response = JObject.Parse(jsonString);
                        if (response["type"].ToString() == "api_token-response" &&
                            response["data"].ToString() == "OK")
                        {
                            // ok, we're authenticated!
                            _wsIsAuthenticated = true;
                        }
                    }
                }
            }).ContinueWith((continuation) => {
              // write any buffered messages
              if (_ws != null && _ws.State == WebSocketState.Open && _wsIsAuthenticated && _buffer.Count > 0)
              {
                  foreach (var b in _buffer)
                      _ws.Send(b);
              }
            });
        }

        public void Trace(params object[] objs)
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                _ws.Send(new ApiExceptionObject("trace", objs).ToString());
            }
            else
            {
                _buffer.Enqueue(new ApiExceptionObject("trace", objs).ToString());
            }
        }
    }
}
