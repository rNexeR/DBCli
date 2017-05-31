using System;
using System.Collections.Generic;

namespace FileDB
{
    public partial class FileDBMS
    {
        private void CreateDBMetadata(){
            LoadBitmap();

            for(var i = 0; i < this.bm_blocks + this.table_list_blocks; i++)
                this.SetUsedBlock(i);

            for(var i = 0; i < this.table_list_blocks; i++){
                this.SetUsedBlock((int)this.bm_blocks + i);
                if(i!=0){
                    this.LinkBlocks((int)this.bm_blocks + i-1, (int)this.bm_blocks + i);
                }
            }

            StoreBitmap();

            PrintDebug($"Next free block: {GetFreeBlock()}");
        }

        private void LoadMetadata(){
            this.LoadBitmap();
            this.LoadTables();
        }

        private void LoadTables(){
            this.table_list = new Dictionary<string, int>();
            var current_pos = 0;
            var lista_bytes = new List<byte>();
            for(var i = 0; i < this.table_list_blocks; i++){
                var arr = this.ReadBlockWithoutLink(this.table_list_blocks + i);
                lista_bytes.AddRange(arr);
            }

            while(lista_bytes.Count > 0){
                var temp = new List<byte>();
                var current = lista_bytes[current_pos];
                while(current != '\0'){
                    temp.Add(lista_bytes[current_pos++]);
                    current = lista_bytes[current_pos];
                }

                var name = this.ToString(temp.ToArray());
                temp = new List<byte>();

                current_pos++;
                
                for(int i = 0; i < 4; i++){
                    temp.Add(lista_bytes[current_pos++]);
                }
                var block = this.ToInt(temp.ToArray());

                if(name != ""){
                    this.table_list[name] = block;
                    PrintDebug($"Discovered table {name} at {block}");
                }

                if(lista_bytes[current_pos++] == '\0' && lista_bytes[current_pos] == '\0'){
                    break;
                }
                if(current_pos >= lista_bytes.Count)
                    break;
            }
        }

        private void StoreDBMetadata(){
            StoreBitmap();
            StoreDBTables();
        }

        private void StoreDBTables(){
            PrintDebug("Store Tables");
            var bytes = new List<byte>();
            foreach(var item in this.table_list){
                bytes.AddRange(this.FromString(item.Key));
                bytes.Add(new byte());
                bytes.AddRange(this.FromInt(item.Value));
                bytes.Add(new byte());
            }

            var emptyBlock = new byte[this.block_size-4];
            this.WriteBlock(emptyBlock, (int)this.bm_blocks, 0);
            this.WriteBlock(emptyBlock, (int)this.bm_blocks+1, 0);

            var current_block = bm_blocks;
            for(var i = 0; i < bytes.Count && current_block < bm_blocks + 2; i+= this.block_size -4){
                PrintDebug($"Writting block {(int)current_block++}");
                this.WriteBlock(bytes.ToArray(), (int)current_block++, i);
            }
        }
    }
}