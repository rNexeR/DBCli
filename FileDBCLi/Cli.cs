using System;
using System.Collections.Generic;
using FileDB;
using Microsoft.Extensions.CommandLineUtils;

namespace FileDBCLi
{
    public class Cli
    {
        private int bs;
        private FileDBMS db;
        private CommandLineApplication app;

        public Cli()
        {
            this.bs = 4096;
            this.db = new FileDBMS(bs);

            this.InitializeCli();

            Console.WriteLine("FileDB by rNexeR");
        }

        private void InitializeCli()
        {
            this.app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "dbcli";
            app.HelpOption("-h|--help");

            app.OnExecute(() =>
            {
                return 0;
            });

            app.Command("use", (command) =>
            {
                command.Description = "Use <dbname>";
                command.HelpOption("-h|--help");

                var nameArgument = command.Argument("[name]", "Name of the Database");

                command.OnExecute(() =>
                {
                    db.UseDB(nameArgument.Value);

                    return 0;
                });

            });

            app.Command("list", (command) =>
            {
                command.Description = "List tables";
                command.HelpOption("-h|--help");

                command.OnExecute(() =>
                {
                    db.ListTables();

                    return 0;
                });

            });

            app.Command("def", (command) =>
            {
                command.Description = "Use <tables>";
                command.HelpOption("-h|--help");

                var nameArgument = command.Argument("[name]", "Name of the Table");

                command.OnExecute(() =>
                {
                    db.DefTable(nameArgument.Value);

                    return 0;
                });

            });

            app.Command("create", (command) =>
            {

                command.Description = "Create <database|table>";
                command.HelpOption("-h|--help");

                var targetArgument = command.Argument("[target]", "Database or Table");
                var nameArgument = command.Argument("[name]", "Name of the new Database or Table");
                var sizeOption = command.Option("-s|--size",
                                "Size of the database.",
                                CommandOptionType.SingleValue);

                var colsDefinition = command.Option("-c|--columns",
                                "Columns of the table.",
                                CommandOptionType.MultipleValue);


                command.OnExecute(() =>
                {
                    if (targetArgument.Value == "database")
                    {
                        db.CreateDatabase(nameArgument.Value, sizeOption.HasValue() ? int.Parse(sizeOption.Value()) : 25);
                    }
                    else
                    {
                        db.CreateTable(nameArgument.Value, colsDefinition.Values);
                    }

                    return 0;
                });

            });

            app.Command("drop", (command) =>
            {

                command.Description = "Drop <database|table> <name>";
                command.HelpOption("-h|--help");

                var targetArgument = command.Argument("[target]", "Database or Table");
                var nameArgument = command.Argument("[name]", "Name of the new Database or Table to be dropped");


                command.OnExecute(() =>
                {
                    if (targetArgument.Value == "database")
                    {
                        db.DropDatabase(nameArgument.Value);
                    }
                    else
                    {
                        db.DropTable(nameArgument.Value);
                    }

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
                    db.InsertRow(tableName.Value, values.Values);

                    return 0;
                });
            });

            app.Command("update", (command) =>
            {
                command.Description = "Update a row into a table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to update");
                var whereOption = command.Option("-w|--where",
                                "What rows are going to change.",
                                CommandOptionType.SingleValue);
                var newValues = command.Argument("[NewValues]", "Values to update", true);

                command.OnExecute(() =>
                {
                    if (!whereOption.HasValue())
                    {
                        db.UpdateRows(tableName.Value, newValues.Values);
                    }
                    else
                    {
                        db.UpdateRowsWhere(tableName.Value, newValues.Values, whereOption.Value());
                    }

                    return 0;
                });
            });

            app.Command("select", (command) =>
            {
                command.Description = "Select row(s) from table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to select");
                var whereOption = command.Option("-w|--where",
                                "What rows do you need.",
                                CommandOptionType.SingleValue);
                var columnsOption = command.Option("-c|--columns",
                                "What cols do you need.",
                                CommandOptionType.MultipleValue);

                var def_value = new List<string>();
                def_value.Add("*");

                command.OnExecute(() =>
                {
                    db.SelectRowsWhere(tableName.Value, whereOption.HasValue() ? whereOption.Value() : "", columnsOption.HasValue() ? columnsOption.Values : def_value);

                    return 0;
                });
            });

            app.Command("delete", (command) =>
            {
                command.Description = "Update a row into a table";
                command.HelpOption("-h|--help");

                var tableName = command.Argument("[table]", "Table to update");
                var whereOption = command.Option("-w|--where",
                                "What rows are going to change.",
                                CommandOptionType.SingleValue);

                command.OnExecute(() =>
                {
                    db.DeleteRowsWhere(tableName.Value, whereOption.HasValue() ? whereOption.Value() : "");

                    return 0;
                });
            });
        }

        public string GetCurrentDB()
        {
            return db.GetCurrentDB();
        }

        public void Execute(string[] args)
        {
            GC.Collect();
            this.InitializeCli();
            app.Execute(args);
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
