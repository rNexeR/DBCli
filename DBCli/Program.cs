using System;
using System.Collections;
using FileDBCLi;

namespace DBCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new Cli();
            var current_db = cli.GetCurrentDB();

            // byte[] bytearray = new byte[16];

            // var bitArray = new BitArray(bytearray);

            // Console.WriteLine($"bit 8 {bitArray.Get(8)}");
            // bitArray.Set(8, true);
            // Console.WriteLine($"bit 8 {bitArray.Get(8)}");

            //  ((ICollection)bitArray).CopyTo(bytearray, 0);

            //  bitArray = new BitArray(bytearray);

            // Console.WriteLine($"bit 8 {bitArray.Get(8)}");
            // Console.WriteLine($"bitArray length = {bitArray.Length}");

            // Console.WriteLine($"sizeof int {sizeof(int)} \nsizeof char {sizeof(char)} \nsizeof double {sizeof(double)}");

            string input;
            string[] arguments;

            while (true)
            {
                Console.Write($"{current_db}>");
                input = "";
                arguments = new string[]{};
                input = Console.ReadLine();
                arguments = input.Split(' ');
                if (input.ToLower() == "exit")
                    break;
                else
                {
                    cli.Execute(arguments);
                    Console.WriteLine();
                    current_db = cli.GetCurrentDB();
                }
            }

            cli.Dispose();
            Console.WriteLine("Bye bye!");
        }
    }
}
