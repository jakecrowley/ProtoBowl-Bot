using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtoBowl_Bot
{
    class Program
    {
        //static WebClient client = new WebClient();

        static void Main(string[] args)
        {
            Console.WriteLine("ProtoBowl Bot v0.01 by Jake Crowley");
            Console.Write("Enter game id: ");

            ProtoBowlClient client = new ProtoBowlClient(Console.ReadLine());
            client.connect();

            client.OnQuestionEvent += (sender, q) =>
            {
                Console.WriteLine("\nQuestion: " + q.Question + "\nAnswer: " + q.Answer);
            };

            Console.ReadLine();
        }
    }
}
