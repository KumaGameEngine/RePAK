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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Usage: view <file>");
                Console.ResetColor();
                return;
            }

            if (!vfs.Exists(args[0]))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"file '{args[0]}' does not exist in the pack file");
                Console.ResetColor();
                return;
            }

            if (vfs.FileTable[args[0]].length > 5 * MEGABYTE)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"file too big for viewing");
                Console.ResetColor();
                return;
            }

            Console.WriteLine(vfs.ReadAlltext(args[0]));
        }
    }
}
