using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace ProtoBowl_Bot
{
    class SocketIOClient : WebsocketClient
    {
        private int mc = 0;

        public SocketIOClient(Uri uri) : base(uri) { }

        public new Task Start() 
        {
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
