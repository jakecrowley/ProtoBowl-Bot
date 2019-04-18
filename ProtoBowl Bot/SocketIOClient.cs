using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Websocket.Client;

namespace ProtoBowl_Bot
{
    class SocketIOClient : WebsocketClient
    {
        private WebClient webclient = new WebClient();
        private Uri websocketuri;
        private int mc = 0;

        public SocketIOClient(Uri uri) : base(uri) {
            string resp = webclient.DownloadString(uri);
            websocketuri = new Uri("ws://ocean.protobowl.com:443/socket.io/1/websocket/" + resp.Split(':')[0]);
        }

        public new Task Start() 
        {
            Url = websocketuri;

            MessageReceived.Subscribe((message) => {
                if (message.Text.StartsWith("2::"))
                {
                    Send("2::");
                }
            });

            return base.Start();
        }

        public void SocketIOSend(string message)
        {
            if (mc == 0)
            {
                Send($"5:::{message}");
            }
            else
            {
                Send($"5:{mc}+::{message}");
            }
            mc++;
        }

    }
}
