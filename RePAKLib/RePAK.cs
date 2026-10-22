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
            outputStream.SetLength(0);

            byte[] keyPadded = new byte[32];

            if (key.Length > 32)
                throw new ArgumentOutOfRangeException("Encryption key must be less then or equal to 32 characters of lenght");

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

            string[] files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

            List<byte> table = new();

            foreach (var item in files)
            {
                var info = new FileInfo(item);
                var fr = Path.GetRelativePath(directory, item).Replace('\\','/');

                var sb = Encoding.ASCII.GetBytes(fr + '\0');

                table.AddRange(sb);

                table.AddRange(BitConverter.GetBytes(currentOffset));
                currentOffset += (ulong)info.Length;

                table.AddRange(BitConverter.GetBytes((ulong)info.Length));
            }

            byte[] tablearr = table.ToArray();

            if (encrypted)
                tablearr = ChaCha20Xor(keyPadded,nonce,tablearr);

            outputStream.Write(tablearr);

            foreach (var item in files)
            {
                var fs = File.OpenRead(item);
                fs.CopyTo(outputStream);
                fs.Close();
            }

            outputStream.Position = pos;
            outputStream.Write(BitConverter.GetBytes(table.Count));

            outputStream.Flush();
        }

        public static RePAKVFS Decrypt(FileStream inputStream, string key = "") 
        {
            byte[] keyPadded = new byte[32];

            if (key.Length > 32)
                throw new ArgumentOutOfRangeException("Decryption key must be less then or equal to 32 characters of lenght");

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

                if (!CryptographicOperations.FixedTimeEquals(keyraw, computedKey))
                    throw new Exception("invalid key for RePAK file");
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

            Dictionary<string, (ulong, ulong)> fileTable = new();

            int pathbyte = 0;
            while (pathbyte < size)
            {
                int end = Array.IndexOf(tableRaw, (byte)0, pathbyte);
                string path = Encoding.ASCII.GetString(tableRaw, pathbyte, end - pathbyte);
                pathbyte = end + 1;

                ulong start = BinaryPrimitives.ReadUInt64LittleEndian(tableRaw.AsSpan(pathbyte, 8));
                pathbyte += 8;

                ulong length = BinaryPrimitives.ReadUInt64LittleEndian(tableRaw.AsSpan(pathbyte, 8));
                pathbyte += 8;

                fileTable[path] = (start + size + 128, length);
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
