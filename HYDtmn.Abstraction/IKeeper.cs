using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDtmn.Abstraction
{
    public interface IKeeper
    {
        void Init(string command,bool isconsole=false);

       
        void Run(string command);
        void Cleanup();
    }

    public interface IKeeperNext
    {
        void Init(string command,BlockingCollection<string> cmdlist);


        void Run(string command, BlockingCollection<string> cmdlist);
        void Cleanup();
    }
}
