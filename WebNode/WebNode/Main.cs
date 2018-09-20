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
        private bool isHost;
        public Network currentNetwork;
        private Server server;

        public NodeClient(string password, string name = "user")
        {
            if (!File.Exists("userConf.json")) File.WriteAllLines("userConf.json", new string[] { Crypto.PasswordEncrypt.Encrypt(JsonConvert.SerializeObject(WebNodeTools.GenerateUserConfProduct(name)), password) });
            var confObject = JsonConvert.DeserializeObject<userConfProduct>(Crypto.PasswordEncrypt.Decrypt(File.ReadAllLines("userConf.json")[0], password));
            this.name = confObject.name;
            pubKey = confObject.pubKey;
            privKey = confObject.privKey;
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
            server = new Server(port);
        }
        public void stophost()
        {
            isHost = false;
            server.stop();
        }
    }
}
