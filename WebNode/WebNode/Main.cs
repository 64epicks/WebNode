using System;
using System.Collections.Generic;
using System.Security;
using Newtonsoft.Json;
using System.IO;

namespace WebNode
{
    public class NodeClient
    {
        public string name;
        private string pubKey;
        private string privKey;
        public Network currentNetwork;

        public NodeClient(string password, string name = "user")
        {
            if (!File.Exists("userConf.json")) File.WriteAllLines("userConf.json", new string[] { Crypto.PasswordEncrypt.Encrypt(JsonConvert.SerializeObject(WebNodeTools.GenerateUserConfProduct(name)), password) });
            var confObject = JsonConvert.DeserializeObject<userConfProduct>(Crypto.PasswordEncrypt.Decrypt(File.ReadAllLines("userConf.json")[0], password));
            this.name = confObject.name;
            pubKey = confObject.pubKey;
            privKey = confObject.privKey;
            confObject = null;

        }
        public void createNetwork(string name, string contact)
        {
            currentNetwork = WebNodeTools.CreateNetwork(name, contact);
        }
        public void saveNetwork()
        {
            var prod = WebNodeTools.networkToProduct(currentNetwork);
            File.WriteAllLines(currentNetwork.name + ".json", new string[] { JsonConvert.SerializeObject(prod) });
        }
        public void loadNetworkFromFile(string name)
        {
            currentNetwork = WebNodeTools.loadNetworkFromFile(name);
        }
    }
}
