using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;

namespace ProtoBowl_Bot
{
    public delegate void QuestionEventHandler(object sender,
                                      QuestionEventArgs e);

    class ProtoBowlClient
    {
        private static Properties.ProtoBowlClient settings = Properties.ProtoBowlClient.Default;
        public event QuestionEventHandler OnQuestionEvent;

        private SocketIOClient client;
        private string roomname;
        private string userid;
        private QuestionEventArgs lastq;

        public ProtoBowlClient(string roomname)
        {
            if (settings.Cookie == "")
            {
                settings.Cookie = "PB4CL" + RandomString(36);
                settings.Save();
            }
            
            this.roomname = roomname;
        }

        public void connect()
        {
            client = new SocketIOClient(new Uri("http://ocean.protobowl.com:443/socket.io/1/"));
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
            try
            {
                if (message.IndexOf("{") > -1)
                {
                    message = message.Substring(message.IndexOf("{"));
                }
                
                SocketMessage json = JsonConvert.DeserializeObject<SocketMessage>(message);

                if (json.name == "log" && userid == null)
                {
                    userid = json.args[0].user;
                    Console.WriteLine("Got user id: " + userid);
                }


                string questionid = json.args[0].qid;
                string question = json.args[0].question;
                string answer = json.args[0].answer;
                double endtime = json.args[0].end_time;

                QuestionEventArgs q = new QuestionEventArgs(questionid, question, answer, endtime);
                if (!q.Equals(lastq) && !q.isEmpty()) {
                    lastq = q;
                    OnQuestionEvent?.Invoke(this, q);
                }
            }
            catch (Exception e) { }
        }

        public void sendChatMessage(string message)
        {
            string session = RandomString(10);
            string msg = "{\"name\":\"chat\",\"args\":[{\"text\":\""+message+"\",\"session\":\""+session+"\",\"user\":\""+userid+"\",\"first\":true,\"done\":true}]}";
            client.SocketIOSend(msg);
        }

        public void sendAnswer(string answer)
        {
            if (lastq != null)
            {
                /*
                if(currentTime() > lastq.EndTime)
                {
                    Console.WriteLine($"Invalid buzz, ct: {currentTime()}, et: {lastq.EndTime}");
                    return;
                }
                */

                client.SocketIOSend("{\"name\":\"buzz\",\"args\":[\"" + lastq.QuestionID + "\"]}");
                client.SocketIOSend("{\"name\":\"guess\",\"args\":[{\"text\":\""+answer+"\",\"done\":true},null]}");
            }
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

        private long currentTime()
        {
           return (long)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
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
        public string QuestionID { get; }
        public string Question { get; }
        public string Answer { get; }
        public double EndTime { get; }

        public QuestionEventArgs(String qid, String question, String answer, double endtime)
        {
            QuestionID = qid;
            Question = question;
            Answer = answer;
            EndTime = endtime;
        }

        public bool isEmpty()
        {
            return QuestionID.Length == 0 || Question.Length == 0 || Answer.Length == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(QuestionEventArgs))
                return false;

            QuestionEventArgs other = (QuestionEventArgs)obj;
            return QuestionID.Equals(other.QuestionID);
        }
    }

    public class SocketMessage
    {
        [JsonProperty]
        public string name;

        [JsonProperty]
        public dynamic args;
    }
}
