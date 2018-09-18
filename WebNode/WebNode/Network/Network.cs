using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebNode
{
    public class Network
    {
        public string name;
        public string contact;
        public bool isOwner;
        public Node[] nodes;
        public string pubKey;
        public string privKey;
        public string[] contentLookAt;

        public Network(string name, string contact, bool isOwner, string[] nodes, string[] contentLookAt, string pubKey, string privKey = "")
        {
            this.name = name;
            this.contact = contact;
            this.isOwner = isOwner;
            this.pubKey = pubKey;
            this.privKey = privKey;
            this.contentLookAt = contentLookAt;

            var nodeList = new List<Node>();
            foreach(string node in nodes)
            {
                string[] nodeInfo = node.Split('|');
                nodeList.Add(new Node(nodeInfo[0], nodeInfo[1], Int32.Parse(nodeInfo[2]), nodeInfo[3], Int32.Parse(nodeInfo[4])));
            }
            this.nodes = nodeList.ToArray();

        }
    }
    public class NetworkProduct
    {
        public string name { get; set; }
        public string contact { get; set; }
        public bool isOwner { get; set; }
        public string[] contentLookAt { get; set; }
        public string pubKey { get; set; }
        public string privKey { get; set; }
        public string[] nodes { get; set; }
    }
}
