using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FileDB
{
    public partial class FileDBMS
    {
        private int block_size;
        private FileStream current_db_file;
        private string current_db_name;
        private BitArray bitmap;
        private long bm_blocks;
        private int table_list_blocks;
        private Dictionary<string, int> table_list;

        public FileDBMS(int block_size)
        {
            this.block_size = block_size;
            this.table_list_blocks = 2;
            if (!Directory.Exists("dbs"))
                Directory.CreateDirectory("dbs");
            this.current_db_name = "";
            this.table_list = new Dictionary<string, int>();
        }

        private string GetPath(string name)
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
            // Console.WriteLine($"Next free block: {GetFreeBlock()}");
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

        public void CreateTable(string tablename, List<string> cols)
        {
            throw new NotImplementedException();
        }

        public void DropDatabase(string dbname)
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

        public void DropTable(string tablename)
        {
            throw new NotImplementedException();
        }

        public void InsertRow(string tablename, List<string> values)
        {
            throw new NotImplementedException();
        }

        public void UpdateRows(string tablename, List<string> values)
        {
            throw new NotImplementedException();
        }

        public void UpdateRowsWhere(string tablename, List<string> values, string where)
        {
            throw new NotImplementedException();
        }

        public void DeleteRowsWhere(string tablename, string where)
        {
            throw new NotImplementedException();
        }

        public void DeleteRows(string value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this.current_db_name != "")
            {
                this.StoreBitmap();
                this.current_db_name = "";
                current_db_file.Flush();
                current_db_file.Dispose();
            }
        }

        public string GetCurrentDB()
        {
            return current_db_name;
        }
    }
}
