using System;
using System.Collections.Generic;

namespace FileDB
{
    public partial class FileDBMS
    {
        private Dictionary<string, column> ParseTableDef(string tablename) //DONE
        {
            this.LoadTableDefinition(tablename);
            var def = this.table_defs[tablename];
            var columns = def.Split(',');

            string col_name;
            string col_type;
            int col_long;

            var ret = new Dictionary<string, column>();

            foreach(var col in columns){
                var column = col.Split(':');
                col_name = column[0];
                if(column[1] == "int"){
                    col_type = column[1];
                    col_long = 4;
                }else if(column[1] == "double"){
                    col_type = column[1];
                    col_long = 8;
                }else{
                    var col_def = column[1].Split('(');
                    col_type = col_def[0];
                    col_long = int.Parse(col_def[1].Remove(col_def[1].Length -1));
                }
                ret[col_name] = new column(col_type, col_long);
            }
            return ret;
        }

        private void LoadTableDefinition(string tablename) //DONE
        {
            if(this.table_defs.ContainsKey(tablename))
                return;

            var block_def = this.table_list[tablename];
            var bytes_def = this.ReadBlockWithoutLink(block_def);

            var len_bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                len_bytes[i] = bytes_def[i];
            }

            var len_def = ToInt(len_bytes);

            var def = new byte[len_def];
            for (int i = 0; i < len_def; i++)
            {
                def[i] = bytes_def[i + 8];
            }

            var def_string = ToString(def);
            this.table_defs[tablename] = def_string;
            // Print(def_string);
        }

        private List<byte[]> SelectAll(string tablename)
        {
            var block_def = this.table_list[tablename];
            var c_block_content = this.ReadBlockWithoutLink(block_def);

            var len_bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                len_bytes[i] = c_block_content[i];
            }

            var len_def = ToInt(len_bytes);

            var rows_count = this.GetTableRowsCount(tablename);
            PrintDebug($"Rows: {rows_count}");
            var len_row = GetTableRowLen(tablename);
            PrintDebug($"Row Len: {len_row}");
            PrintDebug($"Def Len: {len_def}");

            var c_row = 8 + len_def;
            var c_block = block_def;
            var rows = new List<byte>();
            while(c_block != 0){
                c_block_content = this.ReadBlock(c_block);

                if(c_block == block_def){
                    PrintDebug("Metadata and data");
                    for(int i = len_def + 8; i < this.block_size -4; i++){
                        rows.Add(c_block_content[i]);
                    }
                }else{
                    var temp = new byte[this.block_size -4];
                    Array.Copy(c_block_content, temp, this.block_size-4);
                    rows.AddRange(temp);
                }

                
                var next_block_bytes = new byte[4];
                for (int i = 0; i < 4; i++)
                    next_block_bytes[i] = c_block_content[this.block_size - 4 + i];
                c_block = this.ToInt(next_block_bytes);
            }

            PrintDebug($"Total bytes: {rows.Count}");

            var ret = new List<byte[]>();

            for(var i = 0; i < rows_count; i++){
                var row = new byte[len_row+1];
                for(var j = 0; j < len_row+1; j++){
                    row[j] = rows[(i*(len_row+1)) + j];
                }
                ret.Add(row);
            }

            return ret;
        }

        private int GetTableRowsCount(string tablename)
        {
            var block_def = this.table_list[tablename];
            var c_block_content = this.ReadBlockWithoutLink(block_def);

            var rows_count_bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                rows_count_bytes[i] = c_block_content[i+4];
            }

            var rows_count = ToInt(rows_count_bytes);
            // Print($"Rows: {rows_count}");

            return rows_count;
        }

        private int GetTableRowLen(string tablename)
        {
            var t_def = ParseTableDef(tablename);

            int len = 0;
            foreach(var item in t_def){
                len += item.Value.longitud;
            }
            return len;
        }

        private int GetTableDefLen(string tablename){
            var block_def = this.table_list[tablename];
            var bytes_def = this.ReadBlockWithoutLink(block_def);

            var len_bytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                len_bytes[i] = bytes_def[i];
            }

            var len_def = ToInt(len_bytes);

            return len_def;
        }
    }
}