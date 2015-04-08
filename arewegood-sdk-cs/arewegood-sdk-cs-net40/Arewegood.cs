using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using Zeroconf;
using System.Security;
using System.Threading.Tasks;
using System.Net;

namespace arewegood
{
    public class Arewegood
    {
        private static int WS_TIMEOUT_MS = 30 * 1000;

        private String _apiKey;
        private string _serviceName;
        private WebSocket _ws;
        private bool _wsIsAuthenticated;
        private Queue<string> _buffer;

        private bool _wsGaveUp = false;

        public event SocketUpHandler SocketUp;
        public delegate void SocketUpHandler();

        public Arewegood(string apiKey, string serviceName = "_awgproxy._tcp.local.")
        {
            
            _serviceName = serviceName;
            _wsIsAuthenticated = false;
            _buffer = new Queue<string>();

            // resolve the service and test/find the first valid websocket
            // authenticate it, to


            _apiKey = apiKey;

            Task.Delay(WS_TIMEOUT_MS).ContinueWith((continuation) =>
                {
                    _wsGaveUp = true;
                    JObject obj = new JObject();
                    obj["logs"] = new JArray();
                    foreach (var buf in _buffer)
                    {
                        ((JArray)obj["logs"]).Add(buf);
                    }
                    MakeRawHttp(obj.ToString());
                    _buffer.Clear();
                });

            try
            {
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
                        _buffer.Clear();
                    }
                });
            } catch (TypeInitializationException)
            {
                _wsGaveUp = true;
            }
        }


       private HttpStatusCode MakeHttp(string sev, params object[] objs)
       {
           var obj = new ApiExceptionObject(sev, objs);
           JObject o = new JObject();
           o["logs"] = new JArray();
           ((JArray)o["logs"]).Add(obj);

           return MakeRawHttp(o.ToString());
       }

       private HttpStatusCode MakeRawHttp(string body)
       {
           var req = WebRequest.CreateHttp("https://api.arewegood.io/logs?access_token="+_apiKey);
           req.Method = "POST";
           req.ContentType = "application/json";
           var stream = req.GetRequestStream();
           var bytes = System.Text.Encoding.UTF8.GetBytes(body);

           stream.Write(bytes, 0, bytes.Length);
           stream.Close();
           try
           {
               var res = (HttpWebResponse)req.GetResponse();
               return res.StatusCode;
           }
           catch (WebException e)
           {
               return HttpStatusCode.Unauthorized;
           }
       }

        private void InternalLogAsync(string type, params object[] objs)
        {
            if (_ws != null && _ws.State == WebSocketState.Open)
            {
                _ws.Send(new ApiExceptionObject(type, objs).ToString());
            }
            else if (!_wsGaveUp)
            {
                _buffer.Enqueue(new ApiExceptionObject(type, objs).ToString());
            }
            else
            {
                MakeHttp(type, objs);
            }
        }


        private void InternalLogSync(string type, params object[] objs)
        {
            if (!_wsGaveUp)
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
            else
            {
                MakeHttp(type, objs);
            }
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
