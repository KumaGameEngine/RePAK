using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.CLI
{
    public class PackCommand : ICLICommand
    {
        public void Execute(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Console.WriteLine("Usage: pack <directory> <output.pak> [key]");
                return;
            }

            var fs = File.Create(args[1]);

            RePAK.Encrypt(fs, args[0], args.Length > 2 ? args[2] : "");

            fs.Close();
        }
    }
}
