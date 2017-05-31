using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

namespace FileDB
{
    public partial class FileDBMS
    {
        private void LoadBitmap()
        {
            if (this.current_db_name == "")
                return;

            var dbsize = current_db_file.Length;
            this.bm_blocks = (dbsize / this.block_size) / (this.block_size*8);

            if (this.bm_blocks * this.block_size < dbsize)
                this.bm_blocks++;

            var bitmap = new List<byte>();

            for (var i = 0; i < this.bm_blocks; i++)
            {
                current_db_file.Seek(i * this.block_size, SeekOrigin.Begin);
                byte[] arr = new byte[this.block_size];
                current_db_file.Read(arr, 0, this.block_size);
                bitmap.AddRange(arr);
            }

            this.bitmap = new BitArray(bitmap.ToArray());

        }

        private void StoreBitmap()
        {
            if (this.current_db_name == "")
                return;

            byte[] arr = new byte[this.bitmap.Length / 8];
            ((ICollection)bitmap).CopyTo(arr, 0);

            for (var i = 0; i < this.bm_blocks; i++)
            {
                current_db_file.Seek(i * this.block_size, SeekOrigin.Begin);
                current_db_file.Write(arr, i * this.block_size, this.block_size);
            }
        }

        private int GetFreeBlock()
        {
            for (var i = 0; i < this.bitmap.Length; i++)
            {
                if (!bitmap.Get(i))
                    return i;
            }

            throw new FileDBException($"No Available Space on database {current_db_name}.");
        }

        private void SetUsedBlock(int pos)
        {
            this.bitmap.Set(pos, true);
        }

        private void SetUnusedBlock(int pos)
        {
            var emptyBlock = new byte[this.block_size-4];
            this.WriteBlock(emptyBlock, pos, 0);
            this.LinkBlocks(pos, 0);
            this.bitmap.Set(pos, false);
        }

    }
}