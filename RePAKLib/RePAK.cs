using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace RePAKLib
{
    public static class RePAK
    {
        public static void Encrypt(FileStream outputStream, string directory, string key = "")
        {
            byte[] keyPadded = new byte[32];

            for (int i = 0; i < Math.Min(key.Length, keyPadded.Length); i++)
                keyPadded[i] = (byte)key[i];

            outputStream.Write(Encoding.ASCII.GetBytes("repk"));

            outputStream.WriteByte(1);
            outputStream.WriteByte(0);

            bool encrypted = key.Length != 0;

            outputStream.WriteByte((byte)(encrypted ? 1 : 0));

            outputStream.Write(SHA512.HashData(keyPadded));

            var nonce = RandomNumberGenerator.GetBytes(12);
            outputStream.Write(nonce);

            var pos = outputStream.Position;

            while (outputStream.Position < 128)
                outputStream.WriteByte(0);

            ulong currentOffset = 0;

            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                            .Select(f => Path.GetRelativePath(directory, f))
                            .ToArray();

            List<byte> table = new();

            foreach (var item in files)
            {
                var info = new FileInfo(item);
                var sb = Encoding.Default.GetBytes(item + '\0');

                table.AddRange(sb);

                table.AddRange(BitConverter.GetBytes(currentOffset));
                currentOffset += (uint)info.Length;

                table.AddRange(BitConverter.GetBytes((uint)info.Length));
            }

            byte[] tablearr = table.ToArray();

            if (encrypted)
                tablearr = ChaCha20Xor(keyPadded,nonce,tablearr);

            outputStream.Write(tablearr);

            foreach (var item in files)
                outputStream.Write(File.ReadAllBytes(item));

            outputStream.Position = pos;
            outputStream.Write(BitConverter.GetBytes(table.Count));

            outputStream.Flush();
        }

        public static RePAKVFS Decrypt(FileStream inputStream, string key = "") 
        {
            byte[] keyPadded = new byte[32];

            for (int i = 0; i < Math.Min(key.Length, keyPadded.Length); i++)
                keyPadded[i] = (byte)key[i];

            byte[] magic = new byte[4];
            inputStream.ReadExactly(magic);

            if (Encoding.ASCII.GetString(magic) != "repk") throw new Exception("invalid RePAK file");

            var major = inputStream.ReadByte();
            var minor = inputStream.ReadByte();

            var flags = inputStream.ReadByte();
            bool encrypted = (flags & 1 << 0) != 0;

            byte[] keyraw = new byte[64];
            inputStream.ReadExactly(keyraw);

            if (encrypted)
            {
                byte[] computedKey = SHA512.HashData(keyPadded);

                for (int i = 0; i < Math.Min(keyraw.Length,computedKey.Length); i++)
                {
                    if (keyraw[i] != computedKey[i])
                        throw new Exception("invalid key for RePAK file");
                }
            }

            byte[] nonce = new byte[12];
            inputStream.ReadExactly(nonce);

            byte[] sizeraw = new byte[4];
            inputStream.ReadExactly(sizeraw);
            uint size = BinaryPrimitives.ReadUInt32LittleEndian(sizeraw);

            inputStream.Position = 128;

            var tableRaw = new byte[size];
            inputStream.ReadExactly(tableRaw);

            if (encrypted)
                tableRaw = ChaCha20Xor(keyPadded,nonce,tableRaw);

            Dictionary<string, (ulong offset, uint lenght)> fileTable = new();

            string path = "";

            for (int i = 0; i < size; i++)
            {
                var ch = (char)tableRaw[i];

                if (ch == '\0')
                {
                    var start = BinaryPrimitives.ReadUInt64LittleEndian(tableRaw.AsSpan(i, 8));
                    i += 8;

                    var lenght = BinaryPrimitives.ReadUInt32LittleEndian(tableRaw.AsSpan(i, 4));
                    i += 4;

                    fileTable[path] = (start + size + 128, lenght);

                    path = "";
                }
                else path += ch;
            }

            return new(fileTable, inputStream);
        }

        static byte[] ChaCha20Xor(byte[] key, byte[] nonce, byte[] data)
        {
            var engine = new ChaCha7539Engine();
            engine.Init(true, new ParametersWithIV(new KeyParameter(key), nonce));
            var output = new byte[data.Length];
            engine.ProcessBytes(data, 0, data.Length, output, 0);
            return output;
        }
    }
}
