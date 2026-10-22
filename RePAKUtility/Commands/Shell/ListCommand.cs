using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.Shell
{
    public class ListCommand : IShellCommand
    {
        const double GIGABYTE = 1024 * 1024 * 1024;
        const double MEGABYTE = 1024 * 1024;
        const double KILOBYTE = 1024;

        const double GIGABYTE_TRESHOLD = 0.9 * GIGABYTE;
        const double MEGABYTE_TRESHOLD = 0.7 * MEGABYTE;
        const double KILOBYTE_TRESHOLD = 1.7 * KILOBYTE;

        public void Execute(RePAKVFS vfs, string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("too many arguments");
                return;
            }

            var longest = vfs.FileTable.Keys.MaxBy(x => x.Length)!.Length;

            foreach (var item in vfs.FileTable)
            {
                Console.Write(item.Key);
                Console.Write(new string(' ', longest + 4 - item.Key.Length));

                var fs = item.Value.length;

                if (fs >= GIGABYTE_TRESHOLD)
                    Console.WriteLine($"{(fs / GIGABYTE):F2} GB");
                else if (fs >= MEGABYTE_TRESHOLD)
                    Console.WriteLine($"{(fs / MEGABYTE):F2} MB");
                else if (fs >= KILOBYTE_TRESHOLD)
                    Console.WriteLine($"{(fs / KILOBYTE):F2} KB");
                else 
                    Console.WriteLine($"{fs} Byte");
            }
        }
    }
}
