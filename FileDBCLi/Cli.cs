using System;
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
                                "Size of the database.",
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

                    if (whereOption.HasValue())
                    {
                        db.DeleteRowsWhere(tableName.Value, whereOption.Value());
                    }
                    else
                    {
                        db.DeleteRows(tableName.Value);
                    }

                    return 0;
                });
            });
        }

        public string GetCurrentDB(){
            return db.GetCurrentDB();
        }

        public void Execute(string[] args)
        {
            this.InitializeCli();
            app.Execute(args);
        }

        public void Dispose(){
            db.Dispose();
        }
    }
}
