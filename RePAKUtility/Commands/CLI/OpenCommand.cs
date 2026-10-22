using RePAKLib;
using RePAKUtility.Commands.Shell;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility.Commands.CLI
{
    public class OpenCommand : ICLICommand
    {
        static Dictionary<string,IShellCommand> ShellCommands = new()
        {
            {"list", new ListCommand()},
            {"view", new ViewCommand()},
            {"exit", new ExitCommand()},
        };

        public void Execute(string[] args)
        {
            if (args.Length < 1 || args.Length > 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Usage: open <file.pak> [key]");
                Console.ResetColor();
                return;
            }

            var vfs = RePAK.Decrypt(File.OpenRead(args[0]), args.Length > 1 ? args[1] : "");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(">> ");

                Console.ForegroundColor = ConsoleColor.Cyan;
                var cmdargs = CommandLineParser.SplitCommandLine(Console.ReadLine()!).ToArray();
                Console.ResetColor();

                if (ShellCommands.TryGetValue(cmdargs[0].ToLower(), out var cmd))
                    cmd.Execute(vfs, cmdargs.Skip(1).ToArray());
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid command, available commands: " + string.Join(", ", ShellCommands.Keys));
                    Console.ResetColor();
                }
            }
        }
    }
}
