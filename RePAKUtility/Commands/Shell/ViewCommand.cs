using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.Shell
{
    public class ViewCommand : IShellCommand
    {
        const double MEGABYTE = 1024 * 1024;

        public void Execute(RePAKVFS vfs, string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: view <file>");
                return;
            }

            if (!vfs.Exists(args[0]))
            {
                Console.WriteLine($"file '{args[0]}' does not exist in the pack file");
                return;
            }

            if (vfs.FileTable[args[0]].length > 5 * MEGABYTE)
            {
                Console.WriteLine($"file too big for viewing");
                return;
            }

            Console.WriteLine(vfs.ReadAlltext(args[0]));
        }
    }
}
