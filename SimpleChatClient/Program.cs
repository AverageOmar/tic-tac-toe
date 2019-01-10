using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleChatClient
{
    class Program
    {
        private const string hostname = "127.0.0.1";
        private const int port = 4444;

        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SimpleChatClient _client = new SimpleChatClient();

            if (_client.Connect(hostname, port))
            {
                Console.WriteLine("Connected...");
                
            }
            else
            {
                Console.WriteLine("Failed to connect to: " + hostname + ":" + port);
            }
            Console.Read();


            Application.Run(_client);
        }
    }
}
