using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKLib
{
    public class RePAKVFS(Dictionary<string, (ulong offset, uint lenght)> fileTable, FileStream dataStream)
    {
        public readonly Dictionary<string, (ulong offset, uint lenght)> FileTable = fileTable;
        public FileStream DataStream { get; } = dataStream;

        public SubStream GetFileStream(string path)
        {
            if (!FileTable.TryGetValue(path,out var data)) throw new FileNotFoundException("could not find "+ path);

            return new SubStream(DataStream, (long)data.offset, data.lenght);
        }

        public byte[] ReadAllBytes(string path)
        {
            var str = GetFileStream(path);
            byte[] bytes = new byte[str.Length];

            str.ReadExactly(bytes);

            return bytes;
        }

        public string ReadAlltext(string path) => Encoding.Default.GetString(ReadAllBytes(path));
    }
}
