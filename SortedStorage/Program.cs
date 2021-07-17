using System;

namespace SortedStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            var storage = new StorageManager<string, string>();

            Console.WriteLine("LSM DB\n------");

            while(true)
            {
                Console.WriteLine("[a (add), g (get), q (quit)]>");
                string action = Console.ReadLine();

                if (action.StartsWith("q"))
                {
                    break;
                }

                if (action.StartsWith("a"))
                {
                    var data = action.Split(' ');
                    storage.Add(data[1], data[2]);
                }

                if (action.StartsWith("g"))
                {
                    var data = action.Split(' ');
                    string result = storage.Get(data[1]);

                    if (result == null)
                    {
                        Console.WriteLine("no data found");
                    } 
                    else
                    {
                        Console.WriteLine(result);
                    }
                }
            }

            Console.WriteLine("bye!!");
        }
    }
}
