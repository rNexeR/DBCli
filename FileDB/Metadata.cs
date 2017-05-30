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

            Console.WriteLine($"Next free block: {GetFreeBlock()}");
        }

        private void LoadMetadata(){
            this.LoadBitmap();
            this.LoadTables();
        }

        private void LoadTables(){
            var current_pos = 0;
            var lista_bytes = new List<byte>();
            for(var i = 0; i < this.table_list_blocks; i++){
                var arr = this.ReadBlockWithoutLink(this.table_list_blocks + i);
                lista_bytes.AddRange(arr);
            }

            while(true){
                var temp = new List<byte>();
                var current = lista_bytes[current_pos];
                while(current != '\0'){
                    temp.Add(lista_bytes[current_pos++]);
                }

                var name = this.ToString(temp.ToArray());
                temp = new List<byte>();
                
                for(int i = 0; i < 4; i++){
                    temp.Add(lista_bytes[current_pos++]);
                }
                var block = this.ToInt(temp.ToArray());

                if(name != ""){
                    this.table_list[name] = block;
                    Console.WriteLine($"Discovered table {name} at {block}");
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
            StoreTables();
        }

        private void StoreTables(){

        }
    }
}