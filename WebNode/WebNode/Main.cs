using System;
using System.Collections.Generic;
using WebNode.Host;
using Newtonsoft.Json;
using System.IO;

namespace WebNode
{
    public class NodeClient
    {
        public string name;
        public string pubKey;
        private string privKey;
        private bool isHost;
        public Network currentNetwork;
        private Server server;
        private KeyProduct key = new KeyProduct();

        public NodeClient(string password, string name = "user")
        {
            if (!File.Exists("userConf.json")) File.WriteAllLines("userConf.json", new string[] { Crypto.PasswordEncrypt.Encrypt(JsonConvert.SerializeObject(WebNodeTools.GenerateUserConfProduct(name)), password) });
            var confObject = JsonConvert.DeserializeObject<userConfProduct>(Crypto.PasswordEncrypt.Decrypt(File.ReadAllLines("userConf.json")[0], password));
            this.name = confObject.name;
            pubKey = confObject.pubKey;
            privKey = confObject.privKey;

            key.cookie = pubKey;
            key.key = privKey;

            confObject = null;
            isHost = false;
        }
        public void createNetwork(string name, string contact)
        {
            currentNetwork = WebNodeTools.CreateNetwork(name, contact);
            if(!Directory.Exists("networks")) Directory.CreateDirectory("networks");
            Directory.CreateDirectory("networks/" + currentNetwork.name);
        }
        public void saveNetwork()
        {
            var prod = WebNodeTools.networkToProduct(currentNetwork);
            File.WriteAllLines("networks/" + currentNetwork.name + "/network" + ".json", new string[] { JsonConvert.SerializeObject(prod) });
        }
        public void loadNetworkFromFile(string name)
        {
            currentNetwork = WebNodeTools.loadNetworkFromFile(name);
        }
        public void host(int port = 54545)
        {
            if (isHost) throw new Exception("Already hosting!"); 
            isHost = true;
            server = new Server(port, key);
        }
        public void stophost()
        {
            isHost = false;
            server.stop();
        }
    }
}
