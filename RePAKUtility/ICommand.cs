using RePAKLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RePAKUtility
{
    public interface ICLICommand
    {
        public void Execute(string[] args);
    }
    public interface IShellCommand
    {
        public void Execute(RePAKVFS vfs, string[] args);
    }
}
