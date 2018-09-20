using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace WebNode
{
    class WebNodeTools
    {
        public static Network CreateNetwork(string name, string emailAdress)
        {
            var rsa = new RSACryptoServiceProvider(4096);

            return new Network(name, emailAdress, true, new string[] { }, new string[] { }, rsa.ToXmlString(false), rsa.ToXmlString(true));

        }
        public static userConfProduct GenerateUserConfProduct(string name)
        {
            var usrConf = new userConfProduct();

            usrConf.name = name;

            var rsa = new RSACryptoServiceProvider(4096);

            usrConf.pubKey = rsa.ToXmlString(false);
            usrConf.privKey = rsa.ToXmlString(true);

            return usrConf;
        }
        public static Network loadNetworkFromFile(string name)
        {
            var netObj = JsonConvert.DeserializeObject<NetworkProduct>(File.ReadAllLines("networks/" + name + "/network" + ".json")[0]);
            return new Network(netObj.name, netObj.contact, netObj.isOwner, netObj.nodes, netObj.contentLookAt, netObj.pubKey, netObj.privKey);
        }
        public static NetworkProduct networkToProduct(Network network)
        {
            var netProd = new NetworkProduct();
            netProd.name = network.name;
            netProd.contact = network.contact;
            netProd.isOwner = network.isOwner;
            netProd.pubKey = network.pubKey;
            netProd.privKey = network.privKey;
            netProd.contentLookAt = network.contentLookAt;

            var nodeList = new List<string>();
            foreach(Node node in network.nodes)
            {
                nodeList.Add(node.name + "|" + node.ip + "|" + node.port + "|" + node.pubKey + "|" + node.lastComm);
            }
            netProd.nodes = nodeList.ToArray();
            return netProd;
        }
    }
    class userConfProduct
    {
        public string name { get; set; }
        public string pubKey { get; set; }
        public string privKey { get; set; }
    }
}
