using System;
using System.Linq;

namespace ProtoBowl_Bot
{
    class Program
    {
        static void Main(string[] args)
        { 
            Console.WriteLine("ProtoBowl Bot v0.02 by Jake Crowley");
            Console.Write("Enter game id: ");

            ProtoBowlClient client = new ProtoBowlClient(Console.ReadLine());
            client.connect();
            client.setName("AnswerBot");

            client.OnQuestionEvent += (sender, q) =>
            {
                Console.WriteLine("\nQuestion ID: " + q.QuestionID + "\nQuestion: " + q.Question + "\nAnswer: " + q.Answer);

                /*
                string fanswer = q.Answer;
                if (fanswer.IndexOf("[") > 0)
                    fanswer = fanswer.Split('[')[0];
                else if (fanswer.IndexOf("(") > 0)
                    fanswer = fanswer.Split('(')[0];

                if (q.Answer.ToLower().Contains("prompt") || q.Answer.Count(f => f == '{') > 1)
                {
                    fanswer = fanswer.Replace("{", "").Replace("}", "");
                }
                else
                {
                    if (fanswer.IndexOf("{") > -1 && fanswer.IndexOf("}") > -1)
                        fanswer = fanswer.Split('{')[1].Split('}')[0];
                }

                client.sendAnswer(fanswer.Replace("\"", ""));
                */
                

                //client.sendChatMessage("Answer: " + q.Answer);
            };

            Console.ReadLine();
        }
    }
}
