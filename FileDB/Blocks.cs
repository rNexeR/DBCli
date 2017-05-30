using System.IO;

namespace FileDB
{
    public partial class FileDBMS
    {
        private void WriteBlock(byte[] bytes, int block_number, int array_offset){
            this.current_db_file.Seek(block_number*this.block_size, SeekOrigin.Begin);
            this.current_db_file.Write(bytes, array_offset, bytes.Length - array_offset > this.block_size -4 ? this.block_size -4 : bytes.Length - array_offset);
            
            
            // for(int i = 0; i < block_count; i++){
            //     int offset = i*(block_size-4);
            //     var bytes_to_store = bytes;

            //     if( i*(block_size-4) + (block_size -4) > bytes.Length ){
            //         offset = 0;
            //         bytes_to_store = new byte[block_size-4];
            //         for(int y = i*(block_size-4); y < (bytes.Length - (i*(block_size-4)) ); y++ )
            //             bytes_to_store[y - i*(block_size-4)] = bytes[y];
            //     }

            //     this.current_db_file.Write(bytes_to_store, offset, this.block_size-4);
            // }
        }

        private byte[] ReadBlock(int block_number){
            this.current_db_file.Seek(block_number*this.block_size, SeekOrigin.Begin);
            byte[] ret = new byte[this.block_size];
            this.current_db_file.Read(ret, 0, this.block_size);

            return ret;
        }

        private byte[] ReadBlockWithoutLink(int block_number){
            this.current_db_file.Seek(block_number*this.block_size, SeekOrigin.Begin);
            byte[] ret = new byte[this.block_size-4];
            this.current_db_file.Read(ret, 0, this.block_size-4);

            return ret;
        }

        private void LinkBlocks(int first_block, int second_block){
            this.current_db_file.Seek(first_block*this.block_size + (this.block_size -4), SeekOrigin.Begin);
            var second_bytes = this.FromInt(second_block);
            this.current_db_file.Write(second_bytes, 0, 4);
        }
    }
}