using System;

namespace FileDB
{
    internal class FileDBException : Exception
    {
        public FileDBException()
        {
        }

        public FileDBException(string message) : base(message)
        {
        }

        public FileDBException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}