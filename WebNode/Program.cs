using System;
using System.IO;
using System.Security.Cryptography;
using SharpDB;


namespace WebNode
{
    public class Program
    {
		
		static DB db = new DB(Environment.CurrentDirectory);

		static void Main(string[] args){
			if (!Directory.Exists("db")) {
				Console.WriteLine("No connection found, do you want to connect to a network or create one?(Y = Create/N = Connect)");
				string input = Console.ReadLine().ToUpper();
				if (input == "N") Join();
				else if (input == "Y") Create();
			}
		}
		static void Join(){
			
		}
		static void Connect(){
			
		}
		static void Create()
		{
			Console.WriteLine("Generating network key...");

			var csp = new RSACryptoServiceProvider(2048);

			var privKey = csp.ExportParameters(true);

			var pubKey = csp.ExportParameters(false);

			string pubKeyString;
			{
				//we need some buffer
				var sw = new StringWriter();
				//we need a serializer
				var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
				//serialize the key into the stream
				xs.Serialize(sw, pubKey);
				//get the string from the stream
				pubKeyString = sw.ToString();
			}
			string privKeyString;
			{
				//we need some buffer
				var sw = new StringWriter();
				//we need a serializer
				var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
				//serialize the key into the stream
				xs.Serialize(sw, privKey);
				//get the string from the stream
				privKeyString = sw.ToString();
			}
            
			db.CreateDatabase("Net");
			db.EnterDatabase("Net");

			db.CreateTable("NetInfo", "Name;Info");

			db.CreateTable("Nodes", "ID;IP;Port;TST;TC");

			db.Insert("NetInfo", "Name;" + pubKeyString);
			db.Insert("NetInfo", "IsOwner;True");
			db.Insert("NetInfo", "PubKey;" + pubKeyString);
			db.Insert("NetInfo", "PrivKey;" + privKeyString);
			db.Insert("NetInfo", "Nodes;0");

			Directory.CreateDirectory("html");
			File.Create("html/index.html");

			Console.WriteLine(db.Get("Info", "NetInfo", "Name=Name"));
		}
		static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }
    }
}
