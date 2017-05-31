using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FileDB
{
    public class column
    {
        public string type;
        public int longitud;

        public column(string type, int longitud)
        {
            this.type = type;
            this.longitud = longitud;
        }
    }

    public partial class FileDBMS
    {
        private int block_size;
        private FileStream current_db_file;
        private string current_db_name;
        private BitArray bitmap;
        private long bm_blocks;
        private int table_list_blocks;
        private Dictionary<string, int> table_list;
        private Dictionary<string, string> table_defs;
        private bool enable_debug = false;

        public FileDBMS(int block_size)
        {
            this.block_size = block_size;
            this.table_list_blocks = 2;
            if (!Directory.Exists("dbs"))
                Directory.CreateDirectory("dbs");
            this.current_db_name = "";
            this.table_list = new Dictionary<string, int>();
            this.table_defs = new Dictionary<string, string>();
            // this.enable_debug = true;
        }

        private string GetPath(string name) //DONE
        {
            return $"dbs/{name}.db";
        }

        public void UseDB(string dbname) //DONE
        {
            if (!File.Exists(GetPath(dbname)))
            {
                Console.WriteLine($"Database {dbname} doesn't exists. [File: {dbname}.db]");
                return;
            }

            this.Dispose();

            current_db_file = new FileStream(GetPath(dbname), FileMode.Open, FileAccess.ReadWrite);
            current_db_name = dbname;
            LoadMetadata();
            Console.WriteLine($"Using {dbname}");
            PrintDebug($"Next free block: {GetFreeBlock()}");
        }

        public void CreateDatabase(string dbname, int size) //DONE
        {
            if (File.Exists(GetPath(dbname)))
            {
                Console.WriteLine($"Database {dbname} already exists. [File: {dbname}.db]");
                return;
            }

            current_db_file = new FileStream(GetPath(dbname), FileMode.Create, FileAccess.ReadWrite);
            current_db_file.SetLength(size * 1024 * 1024);
            current_db_name = dbname;

            this.CreateDBMetadata();
            this.LoadMetadata();

            // Console.WriteLine($"Next free block: {GetFreeBlock()}");
            Console.WriteLine($"Database {dbname} created.");
            Console.WriteLine($"Using {dbname}");
        }

        public void CreateTable(string tablename, List<string> cols) //DONE
        {
            if (this.table_list.ContainsKey(tablename))
            {
                Console.WriteLine($"Table {tablename} already exists.");
                return;
            }

            var block = this.GetFreeBlock();
            if (block < 0)
            {
                Print("No block available.");
                return;
            }

            var table_definition = string.Join(",", cols);
            PrintDebug($"Def: {table_definition}");

            foreach (var item in cols)
            {
                Console.WriteLine($"=> {item}");
            }

            var table_metadata = new List<byte>();
            var def_bytes = FromString(table_definition);
            var len_bytes = FromInt(def_bytes.Length);
            table_metadata.AddRange(len_bytes);
            table_metadata.AddRange(this.FromInt(0));
            table_metadata.AddRange(def_bytes);

            if (table_metadata.Count > this.block_size - 4)
            {
                Console.WriteLine("Too many columns.");
                return;
            }

            this.table_list[tablename] = block;
            this.SetUsedBlock(block);
            this.WriteBlock(table_metadata.ToArray(), block, 0);

            Console.WriteLine($"Table {tablename} created.");
        }

        public void DropDatabase(string dbname) //DONE
        {
            if (dbname == current_db_name)
            {
                this.Dispose();
                this.current_db_name = "";
                this.current_db_file = null;
            }

            if (File.Exists(GetPath(dbname)))
            {
                File.Delete(GetPath(dbname));
                Console.WriteLine($"Database {dbname} dropped");
            }
            else
            {
                Console.WriteLine("Database not found.");
            }
        }

        public void DropTable(string tablename) //DONE
        {
            var cant_blocks = this.GetTableBlocksCount(tablename);
            var current_block = this.table_list[tablename];
            for (int i = 0; i < cant_blocks; i++)
            {

                var content = this.ReadBlock(current_block);

                this.SetUnusedBlock(current_block);

                var next_block_bytes = new byte[4];

                for (int z = 0; z < 4; z++)
                    next_block_bytes[z] = content[this.block_size - 4 + z];

                current_block = this.ToInt(next_block_bytes);
            }

            this.table_list.Remove(tablename);
            this.StoreDBMetadata();
        }

        public void InsertRow(string tablename, List<string> values) //DONE
        {
            if (!this.table_list.ContainsKey(tablename))
            {
                Print($"Table {tablename} not found.");
                return;
            }

            PrintDebug($"insert {tablename} {string.Join(" ", values.ToArray())}");
            var table_def_dict = this.ParseTableDef(tablename);

            if (values.Count != table_def_dict.Count)
            {
                Print($"{table_def_dict.Count} values expected, {values.Count} provided.");
            }

            var to_store = new List<byte>();
            to_store.Add(new byte());

            var i = 0;
            foreach (var item in table_def_dict)
            {
                PrintDebug($"{item.Key} : {item.Value.type} ({item.Value.longitud})");
                if (item.Value.type == "int")
                {
                    to_store.AddRange(this.FromInt(int.Parse(values[i])));
                }
                else if (item.Value.type == "double")
                {
                    to_store.AddRange(this.FromDouble(double.Parse(values[i])));
                }
                else
                {
                    var char_col = this.FromString(values[i]);
                    if (char_col.Length == item.Value.longitud * 2)
                    {
                        to_store.AddRange(char_col);
                    }
                    else if (char_col.Length < item.Value.longitud * 2)
                    {
                        var temp = new List<byte>(char_col);
                        temp.AddRange(new byte[item.Value.longitud * 2 - char_col.Length]);
                        to_store.AddRange(temp);
                    }
                    else
                    {
                        Print($"Value {values[i]} is too large.");
                        return;
                    }
                }
                i++;
            }

            StoreNewRow(to_store.ToArray(), tablename);
        }

        public void SelectRowsWhere(string tablename, string where, List<string> columns) //DONE
        {
            var has_where = true;
            if (where == "")
                has_where = false;

            var all_cols = columns.Contains("*");

            PrintDebug($"Where: {where}");

            if (!this.table_list.ContainsKey(tablename))
            {
                Print($"Table {tablename} not found.");
                return;
            }

            var where_col = "";
            var where_cond = "";

            if (has_where)
            {
                var where_parts = where.Split('=');
                where_col = where_parts[0];
                where_cond = where_parts[1];
            }

            var rows = this.SelectAll(tablename);
            var cols_def = this.ParseTableDef(tablename);

            foreach (var row in rows)
            {
                var i = 0;
                var row_string = "";
                var print = true;
                foreach (var col in cols_def)
                {
                    if (!all_cols && !columns.Contains(col.Key))
                        continue;

                    var to_parse = new byte[col.Value.longitud];
                    if (row[0] != '\0')
                        print = false;
                    // Array.Copy(row, 1, to_parse, col.Value.longitud);
                    Array.Copy(row, i + 1, to_parse, 0, col.Value.longitud);
                    var col_val = "";

                    if (col.Value.type == "int")
                    {
                        col_val = $"{this.ToInt(to_parse)}";
                        row_string += col_val + " ";
                    }
                    else if (col.Value.type == "double")
                    {
                        col_val = $"{this.ToDouble(to_parse)}";
                        row_string += col_val + " ";
                    }
                    else
                    {
                        col_val = $"{this.ToString(to_parse)}";
                        row_string += col_val + " ";
                    }

                    if (has_where && col.Value.type != "char" && col.Key == where_col && col_val != where_cond)
                        print = false;

                    if (has_where && col.Value.type == "char" && col.Key == where_col && !col_val.Contains(where_cond))
                        print = false;

                    i += col.Value.longitud;
                }
                if (print)
                    Print(row_string);
            }

        }

        public void UpdateRows(string tablename, List<string> values)
        {
            Print("Updating");
            var new_values_dict = new Dictionary<string, string>();
            foreach (var val in values)
            {
                var value_parts = val.Split('=');
                new_values_dict[value_parts[0]] = value_parts[1];
            }

            var rows = this.SelectAll(tablename);
            var cols_def = this.ParseTableDef(tablename);

            foreach (var row in rows)
            {
                var i = 0;
                foreach (var col in cols_def)
                {
                    var to_parse = new byte[col.Value.longitud];
                    Array.Copy(row, i + 1, to_parse, 0, col.Value.longitud);

                    if (new_values_dict.ContainsKey(col.Key))
                    {
                        if (col.Value.type == "int")
                        {
                            var new_value = FromInt(int.Parse(new_values_dict[col.Key]));
                            Array.Copy(new_value, 0, row, i + 1, 4);
                        }
                        else if (col.Value.type == "double")
                        {
                            var new_value = FromDouble(double.Parse(new_values_dict[col.Key]));
                            Array.Copy(new_value, 0, row, i + 1, 8);
                        }
                        else
                        {
                            var new_value = FromString(new_values_dict[col.Key]);
                            var fitted = new byte[col.Value.longitud];
                            Array.Copy(new_value, fitted, new_value.Length);
                            Array.Copy(fitted, 0, row, i + 1, col.Value.longitud);
                        }
                    }

                    i += col.Value.longitud;
                }
            }
            this.StoreRows(tablename, rows);
        }

        public void UpdateRowsWhere(string tablename, List<string> values, string where)
        {
            Print("Update");
            var where_parts = where.Split('=');
            var where_col = where_parts[0];
            var where_cond = where_parts[1];

            var new_values_dict = new Dictionary<string, string>();
            foreach (var val in values)
            {
                var value_parts = val.Split('=');
                new_values_dict[value_parts[0]] = value_parts[1];
            }

            var to_update = new List<int>();

            var rows = this.SelectAll(tablename);
            var cols_def = this.ParseTableDef(tablename);

            var j = 0;
            foreach (var row in rows)
            {
                var i = 0;
                var col_val = "";
                foreach (var col in cols_def)
                {
                    var to_parse = new byte[col.Value.longitud];
                    Array.Copy(row, i + 1, to_parse, 0, col.Value.longitud);

                    if (col.Value.type == "int")
                    {
                        col_val = $"{this.ToInt(to_parse)}";
                    }
                    else if (col.Value.type == "double")
                    {
                        col_val = $"{this.ToDouble(to_parse)}";
                    }
                    else
                    {
                        col_val = $"{this.ToString(to_parse)}";
                    }

                    if (col.Value.type != "char" && col.Key == where_col && col_val == where_cond)
                        to_update.Add(j);

                    if (col.Value.type == "char" && col.Key == where_col && col_val.Contains(where_cond))
                        to_update.Add(j);
                        

                    i += col.Value.longitud;
                }
                j++;
            }
            
            j=0;
            foreach (var row in rows)
            {
                var i = 0;
                foreach (var col in cols_def)
                {
                    var to_parse = new byte[col.Value.longitud];
                    Array.Copy(row, i + 1, to_parse, 0, col.Value.longitud);

                    if (new_values_dict.ContainsKey(col.Key) && to_update.Contains(j))
                    {
                        if (col.Value.type == "int")
                        {
                            var new_value = FromInt(int.Parse(new_values_dict[col.Key]));
                            Array.Copy(new_value, 0, row, i + 1, 4);
                        }
                        else if (col.Value.type == "double")
                        {
                            var new_value = FromDouble(double.Parse(new_values_dict[col.Key]));
                            Array.Copy(new_value, 0, row, i + 1, 8);
                        }
                        else
                        {
                            var new_value = FromString(new_values_dict[col.Key]);
                            var fitted = new byte[col.Value.longitud];
                            Array.Copy(new_value, fitted, new_value.Length);
                            Array.Copy(fitted, 0, row, i + 1, col.Value.longitud);
                        }
                    }

                    i += col.Value.longitud;
                }
                j++;
            }
            this.StoreRows(tablename, rows);

        }

        public void DeleteRowsWhere(string tablename, string where) //DONE
        {
            var has_where = true;
            if (where == "")
                has_where = false;

            PrintDebug($"Where: {where}");

            if (!this.table_list.ContainsKey(tablename))
            {
                Print($"Table {tablename} not found.");
                return;
            }

            var where_col = "";
            var where_cond = "";

            if (has_where)
            {
                var where_parts = where.Split('=');
                where_col = where_parts[0];
                where_cond = where_parts[1];
            }

            var rows = this.SelectAll(tablename);
            var cols_def = this.ParseTableDef(tablename);

            foreach (var row in rows)
            {
                var i = 0;
                foreach (var col in cols_def)
                {
                    var to_parse = new byte[col.Value.longitud];
                    Array.Copy(row, i + 1, to_parse, 0, col.Value.longitud);
                    if (!has_where)
                    {
                        row[0] = Convert.ToByte(true);
                    }

                    var col_val = "";

                    if (col.Value.type == "int")
                    {
                        col_val = $"{this.ToInt(to_parse)}";
                    }
                    else if (col.Value.type == "double")
                    {
                        col_val = $"{this.ToDouble(to_parse)}";
                    }
                    else
                    {
                        col_val = $"{this.ToString(to_parse)}";
                    }

                    if (has_where && col.Key == where_col && col_val == where_cond)
                        row[0] = Convert.ToByte(true);

                    i += col.Value.longitud;
                }
            }
            this.StoreRows(tablename, rows);

        }

        public void Dispose() //DONE
        {
            if (this.current_db_name != "")
            {
                this.StoreDBMetadata();
                this.current_db_name = "";
                this.table_list = new Dictionary<string, int>();
                this.table_defs = new Dictionary<string, string>();
                this.table_list_blocks = 2;
                current_db_file.Flush();
                current_db_file.Dispose();
            }
        }

        public string GetCurrentDB() //DONE
        {
            return current_db_name;
        }

        public void ListTables() //DONE
        {
            if (current_db_file == null)
                return;
            Console.WriteLine("--------");
            foreach (var item in this.table_list)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("--------");
        }

        public void DefTable(string tablename) //DONE
        {
            if (!this.table_list.ContainsKey(tablename))
            {
                Console.WriteLine($"Table {tablename} not found.");
            }

            var table_def_dict = this.ParseTableDef(tablename);

            foreach (var item in table_def_dict)
            {
                Print($"\t{item.Key} : {item.Value.type} ({item.Value.longitud})");
            }
        }

        private void PrintDebug(string msg) //DONE
        {
            if (enable_debug)
                Console.WriteLine(msg);
        }

        private void Print(string msg) //DONE
        {
            Console.WriteLine(msg);
        }
        private void Print(int num) //DONE
        {
            Console.WriteLine(num);
        }
        private void Print(double num) //DONE
        {
            Console.WriteLine(num);
        }
    }
}
