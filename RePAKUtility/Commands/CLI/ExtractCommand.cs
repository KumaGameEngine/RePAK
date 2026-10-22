using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.CLI
{
    public class ExtractCommand : ICLICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: extract <input.pak> <output directory>");
                return;
            }

            var vfs = RePAK.Decrypt(File.OpenRead(args[0]), args.Length > 2 ? args[2] : "");

            if (!Directory.Exists(args[1]))
                Directory.CreateDirectory(args[1]);

            foreach (var item in vfs.FileTable)
            {
                var fp = Path.Combine(args[1], item.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(fp)!);

                var f = File.Create(fp);

                vfs.GetFileStream(item.Key).CopyTo(f);

                f.Flush();
                f.Close();
            }
        }
    }
}
