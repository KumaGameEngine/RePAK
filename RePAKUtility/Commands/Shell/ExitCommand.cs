using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.Shell
{
    public class ExitCommand : IShellCommand
    {
        public void Execute(RePAKVFS vfs, string[] args)
        {
            if (args.Length != 0)
            {
                Console.WriteLine("too many arguments");
                return;
            }

            vfs.DataStream.Close();
            Environment.Exit(0);
        }
    }
}
