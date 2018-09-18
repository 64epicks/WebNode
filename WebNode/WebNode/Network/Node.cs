using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebNode
{
    public class Node
    {
        public string name;
        public string ip;
        public int port;
        public string pubKey;
        public int lastComm;

        public Node(string name, string ip, int port, string pubKey, int lastComm)
        {
            this.name = name;
            this.ip = ip;
            this.port = port;
            this.pubKey = pubKey;
            this.lastComm = lastComm;
        }
    }
}
