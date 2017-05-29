using System;
using Microsoft.Extensions.CommandLineUtils;

namespace Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "dbcli";
            app.HelpOption("-h|--help");

            app.OnExecute(() =>
            {
                Console.WriteLine("rNexeR/DBCli");
                return 0;
            });

            app.Command("create", (command) =>
            {

                command.Description = "Create {database|table}";
                command.HelpOption("-?|-h|--help");

                var targetArgument = command.Argument("[target]", "Database or Table");
                var nameArgument = command.Argument("[name]", "Name of the new Database or Table");


                command.OnExecute(() =>
                {
                    Console.WriteLine($"Create {targetArgument.Value} with name {nameArgument.Value}");

                    return 0;
                });

            });

            app.Command("drop", (command) =>
            {

                command.Description = "Drop {database|table}";
                command.HelpOption("-h|--help");

                var targetArgument = command.Argument("[target]", "Database or Table");
                var nameArgument = command.Argument("[name]", "Name of the new Database or Table to be dropped");


                command.OnExecute(() =>
                {
                    Console.WriteLine($"Drop {targetArgument.Value} with name {nameArgument.Value}");

                    return 0;
                });

            });

            app.Command("insert", (command) =>
            {
                command.Description = "Insert a row into a table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to insert a row");
                var values = command.Argument("[values]", "Values to be inserted", true);

                command.OnExecute(() =>
                {
                    Console.Write($"Insert into {tableName.Value} values :[ ");
                    foreach (var x in values.Values)
                    {
                        Console.Write($"{x} ");
                    }
                    Console.WriteLine("]");
                    return 0;
                });
            });

            app.Command("update", (command) =>
            {
                command.Description = "Update a row into a table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to update");
                var whereOption = command.Option("-w|--where <Selection>",
                                "What rows are going to change.",
                                CommandOptionType.SingleValue);
                var newValues = command.Argument("[NewValues]", "Values to update", true);

                command.OnExecute(() =>
                {
                    Console.Write($"Insert into {tableName.Value} NewValues: [ ");
                    foreach (var x in newValues.Values)
                    {
                        Console.Write($"{x} ");
                    }

                    if (whereOption.HasValue())
                    {
                        Console.WriteLine($"] where {whereOption.Value()}");
                    }
                    else
                    {
                        Console.WriteLine("]");

                    }

                    return 0;
                });
            });

            app.Command("delete", (command) =>
            {
                command.Description = "Update a row into a table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to update");
                var whereOption = command.Option("-w|--where <Selection>",
                                "What rows are going to change.",
                                CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    Console.Write($"Delete from {tableName.Value} ");

                    if (whereOption.HasValue())
                    {
                        Console.WriteLine($"where {whereOption.Value()}");
                    }
                    
                    return 0;
                });
            });



            app.Execute(args);
        }
    }
}
