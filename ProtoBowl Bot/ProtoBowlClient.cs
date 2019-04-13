using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace ProtoBowl_Bot
{
    public delegate void QuestionEventHandler(object sender,
                                      QuestionEventArgs e);

    class ProtoBowlClient
    {
        public event QuestionEventHandler OnQuestionEvent;

        private SocketIOClient client;
        private WebClient webclient = new WebClient();
        private string roomname;
        private Uri websocketuri;
        private string cookie = "PB4CLwn4g0vzwroqm8fzkhsk7engmobn2azxbasui";
        private int mc = 0;

        public ProtoBowlClient(string roomname)
        {
            string resp = webclient.DownloadString("http://ocean.protobowl.com:443/socket.io/1/");
            websocketuri = new Uri("ws://ocean.protobowl.com:443/socket.io/1/websocket/" + resp.Split(':')[0]);
            this.roomname = roomname;
        }

        public void connect()
        {
            client = new SocketIOClient(websocketuri);
            client.ReconnectTimeoutMs = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
            client.ReconnectionHappened.Subscribe(type =>
                Console.WriteLine($"Connection to game established. Waiting for question..."));

            client.DisconnectionHappened.Subscribe(type =>
                Console.WriteLine($"Connection to game was lost. Attempting reconnection..."));

            client.MessageReceived.Subscribe(msg => parseMessage(msg.Text));
            client.Start();

            while (!client.IsRunning)
            {
                Thread.Sleep(10);
            }

            string joinmsg = "{\"name\":\"join\",\"args\":[{\"cookie\":\""+cookie+"\",\"auth\":null,\"question_type\":\"qb\",\"room_name\":\""+roomname+"\",\"muwave\":false,\"agent\":\"M4/Web\",\"agent_version\":\"Sat Sep 02 2017 11:33:43 GMT-0700 (PDT)\",\"referrers\":[\"http://protobowl.com/gamerboi\"],\"version\":8}]}";
            client.SocketIOSend(joinmsg);
        }

        private void parseMessage(string message)
        {
            try
            {
                string question = message.Split(new string[] { "\"question\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\",\"" }, StringSplitOptions.None)[0];
                string answer = message.Split(new string[] { "\"answer\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\",\"" }, StringSplitOptions.None)[0];
                OnQuestionEvent?.Invoke(this, new QuestionEventArgs(question, answer));
            }
            catch (Exception e) { }
        }

        public void setName(string name)
        {
            if (client.IsRunning)
            {
                client.SocketIOSend("{\"name\":\"set_name\",\"args\":[\""+name+"\",null]}");
            }
            else
            {
                throw new Exception("Client is not connected to the server.");
            }
        }
    }

    public class QuestionEventArgs : EventArgs
    {
        public string Question { get; }
        public string Answer { get; }

        public QuestionEventArgs(String question, String answer)
        {
            Question = question;
            Answer = answer;
        }
    }
}
