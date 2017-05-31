using System;
using System.Collections.Generic;

namespace FileDB
{
    public partial class FileDBMS
    {
        private void StoreNewRow(byte[] to_store, string tablename)
        {
            int block_pos = this.GetLastTableDataBlock(tablename);
            var last_table_data_block = this.ReadBlockWithoutLink(block_pos);

            var rows_count = this.GetTableRowsCount(tablename);

            Print($"block to store row: {block_pos}");
            Print($"block siz: {last_table_data_block.Length}");

            int i = this.GetLastBlockOffset(tablename);

            // int i = block_pos == this.table_list[tablename] ? this.GetTableDefLen(tablename) + 8 : 0;
            // for (; ; i++)
            // {
            //     if( last_table_data_block[i++] == '\0' ){
            //         if( last_table_data_block[i] == '\0' ){
            //             i--;
            //             break;
            //         }
            //     }
            // }

            Print($"To store row {i}");

            if (this.block_size - 4 >= i + to_store.Length)
            {
                to_store.CopyTo(last_table_data_block, i);
                WriteBlock(last_table_data_block, block_pos, 0);
            }
            else
            {
                var new_block = GetFreeBlock();
                if (new_block < 0)
                {
                    Print("No block available.");
                    return;
                }

                var new_block_content = new byte[this.block_size - 4];
                for (int j = 0; j < this.block_size - 4; j++)
                    new_block_content[j] = to_store[j + (this.block_size - 4 - i)];

                for (int j = 0; i < this.block_size - 4; j++)
                    last_table_data_block[i++] = to_store[j];

                LinkBlocks(block_pos, new_block);
                WriteBlock(last_table_data_block, block_pos, 0);
                WriteBlock(new_block_content, new_block, 0);
            }

            var metadata = this.ReadBlockWithoutLink(this.table_list[tablename]);

            var new_rows_count = this.FromInt(rows_count + 1);
            new_rows_count.CopyTo(metadata, 4);
            this.WriteBlock(metadata, this.table_list[tablename], 0);

            Print($"Rows: {rows_count + 1}");
            Print("Row inserted.");
        }

        private void StoreRows(string tablename, List<byte[]> rows)
        {
            var len_def = this.GetTableDefLen(tablename);
            var row_len = this.GetTableRowLen(tablename) + 1;
            var cant_blocks = this.GetTableBlocksCount(tablename);

            var arr_bytes = new byte[cant_blocks*(this.block_size-4) - (len_def + 8)];
            var i = 0;
            
            foreach(var row in rows){
                Array.Copy(row, 0, arr_bytes, i, row_len);
                i+=row_len;
            }

            var to_store = new List<byte>();
            var metadata = this.ReadBlockWithoutLink(this.table_list[tablename]);
            var only_metadata = new byte[len_def + 8];
            Array.Copy(metadata, only_metadata, len_def + 8);
            to_store.AddRange(only_metadata);
            to_store.AddRange(arr_bytes);

            var to_store_array = to_store.ToArray();

            var current_block = this.table_list[tablename];
            for(int j = 0; j <cant_blocks; j++){
                this.WriteBlock(to_store_array, current_block, j*(this.block_size-4));

                var content = this.ReadBlock(current_block);
                var next_block_bytes = new byte[4];

                for (int z = 0; z < 4; z++)
                    next_block_bytes[z] = content[this.block_size - 4 + z];

                current_block = this.ToInt(next_block_bytes);
            }
        }

        private int GetLastTableDataBlock(string tablename)
        {
            var f_block = this.table_list[tablename];
            byte[] content = new byte[this.block_size];
            var pos = 0;

            while (f_block > 0)
            {
                pos = f_block;
                content = this.ReadBlock(f_block);
                var next_block_bytes = new byte[4];

                for (int i = 0; i < 4; i++)
                    next_block_bytes[i] = content[this.block_size - 4 + i];

                f_block = this.ToInt(next_block_bytes);
            }

            return pos;
        }

        private int GetLastBlockOffset(string tablename){
            var len_def = this.GetTableDefLen(tablename);
            var rows = this.GetTableRowsCount(tablename);
            var row_len = this.GetTableRowLen(tablename) + 1;
            var block_offset = (8 + len_def + (rows*row_len))%(this.block_size-4);
            return block_offset;
        }

        private int GetTableBlocksCount(string tablename){
            var len_def = this.GetTableDefLen(tablename);
            var rows = this.GetTableRowsCount(tablename);
            var row_len = this.GetTableRowLen(tablename) + 1;
            var total_bytes = len_def + 8 + (rows*(row_len+1));
            var total_blocks = total_bytes / (this.block_size -4);
            if(total_blocks*(this.block_size-4) < total_bytes)
                total_blocks++;
            return total_blocks;
        }
    }
}