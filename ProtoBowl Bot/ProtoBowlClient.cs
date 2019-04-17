using Newtonsoft.Json;
using System;
using System.Linq;
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
        private static Properties.ProtoBowlClient settings = Properties.ProtoBowlClient.Default;
        public event QuestionEventHandler OnQuestionEvent;

        private SocketIOClient client;
        private WebClient webclient = new WebClient();
        private string roomname;
        private Uri websocketuri;
        private string userid;
        private QuestionEventArgs lastq;

        public ProtoBowlClient(string roomname)
        {
            if (settings.Cookie == "")
            {
                settings.Cookie = "PB4CL" + RandomString(36);
                settings.Save();
            }

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

            string joinmsg = "{\"name\":\"join\",\"args\":[{\"cookie\":\""+settings.Cookie+"\",\"auth\":null,\"question_type\":\"qb\",\"room_name\":\""+roomname+"\",\"muwave\":false,\"agent\":\"M4/Web\",\"agent_version\":\"Sat Sep 02 2017 11:33:43 GMT-0700 (PDT)\",\"referrers\":[\"http://protobowl.com/gamerboi\"],\"version\":8}]}";
            client.SocketIOSend(joinmsg);
        }

        private void parseMessage(string message)
        {
            if(message.IndexOf("{") > -1)
            {
                message = message.Substring(message.IndexOf("{"));
            }

            if (message.Contains("user\":\""))
            {
                userid = message.Split(new string[] { "\"user\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\",\"" }, StringSplitOptions.None)[0];
            }
            
            try
            {
                string question = message.Split(new string[] { "\"question\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\",\"" }, StringSplitOptions.None)[0];
                string answer = message.Split(new string[] { "\"answer\":\"" }, StringSplitOptions.None)[1].Split(new string[] { "\",\"" }, StringSplitOptions.None)[0];

                QuestionEventArgs q = new QuestionEventArgs(question, answer);
                if (!q.Equals(lastq)) {
                    OnQuestionEvent?.Invoke(this, q);
                    lastq = q;
                }
            }
            catch (IndexOutOfRangeException e) { }
        }

        public void sendChatMessage(string message)
        {
            string session = RandomString(10);
            string msg = "{\"name\":\"chat\",\"args\":[{\"text\":\""+message+"\",\"session\":\""+session+"\",\"user\":\""+userid+"\",\"first\":true,\"done\":true}]}";
            client.SocketIOSend(msg);
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

        
        
        private string RandomString(int length)
        {
            Random r = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[r.Next(s.Length)]).ToArray());
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

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(QuestionEventArgs))
                return false;

            QuestionEventArgs other = (QuestionEventArgs)obj;
            return Question.Equals(other.Question) && Answer.Equals(other.Answer);
        }
    }
}
