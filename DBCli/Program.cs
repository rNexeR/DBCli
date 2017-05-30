using System;
using FileDBCLi;

namespace DBCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new Cli();
            var current_db = cli.GetCurrentDB();

            while(true){
                Console.Write($"{current_db}>");
                var input = Console.ReadLine();
                var arguments = input.Split(' ');
                if(input.ToLower() == "exit")
                    break;
                cli.Execute(arguments);
                Console.WriteLine();
                current_db = cli.GetCurrentDB();
            }

            cli.Dispose();
            Console.WriteLine("Bye bye!");
            Console.Read();
        }
    }
}
