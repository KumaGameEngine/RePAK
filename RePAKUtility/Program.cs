using RePAKLib;
using RePAKUtility.Commands.CLI;
using System.IO;

namespace RePAKUtility
{
    internal class Program
    {
        static Dictionary<string, ICLICommand> Commands = new()
        {
            {"pack", new PackCommand()},
            {"open", new OpenCommand()},
            {"extract", new ExtractCommand()},
        };

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("avaliable commands: " + string.Join(", ", Commands.Keys));
                Console.ResetColor();
                return;
            }

            if (Commands.TryGetValue(args[0].ToLower(), out var cmd))
                cmd.Execute(args.Skip(1).ToArray());
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid command " + args[0]);
                Console.ResetColor();
            }
        }
    }
}
