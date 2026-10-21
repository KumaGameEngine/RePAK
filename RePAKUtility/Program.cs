using RePAKLib;

namespace RePAKUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            switch (args[0])
            {
                case "pack":
                    var fs = File.Create(args[2]);

                    RePAK.Encrypt(fs, args[1], args.Length > 3 ? args[3] : "");

                    fs.Close();
                    break;
                case "list":
                    var vfs = RePAK.Decrypt(File.OpenRead(args[1]), args.Length > 2 ? args[2] : "");

                    foreach (var item in vfs.FileTable)
                        Console.WriteLine(item.Key);
                    break;
            }
        }
    }
}
