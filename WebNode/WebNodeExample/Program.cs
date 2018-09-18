using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WebNode;

namespace WebNodeExample
{
    class Program
    {
        private static NodeClient nodeClient;
        private static string currentNet = "NONE";
        private static bool passErr = false;

        static void Main(string[] args)
        {
            while (true)
            {
                #region Init
                string name = "user";
                Console.Clear();
                if (!File.Exists("userConf.json"))
                {
                    Console.WriteLine("Generating config...");
                    Console.Write("Enter username: ");
                    name = Console.ReadLine();
                }
                Console.Write("Enter password: ");
                string pass = "";
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        pass += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                        {
                            pass = pass.Substring(0, (pass.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                }
                while (key.Key != ConsoleKey.Enter);
                Console.WriteLine("\nLoading config, please wait...");

                try
                {
                    nodeClient = new NodeClient(pass, name);
                    passErr = false;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Incorrect password! Please try again.");
                    passErr = true;
                }
                #endregion
                while (true)
                {
                    if (passErr == true) break;
                    Console.Write(">");
                    string input = Console.ReadLine();

                    if (input == "restart") break;
                    switch (input)
                    {
                        case "create":
                            {
                                Console.Write("Enter name for new network: ");
                                string nameNet = Console.ReadLine();
                                Console.Write("Enter email for contact: ");
                                string conNet = Console.ReadLine();
                                Console.Write("Creating network... ");
                                nodeClient.createNetwork(nameNet, conNet);
                                Console.WriteLine("DONE");
                                break;
                            }
                        case "save":
                            {
                                Console.Write("Saving... ");
                                nodeClient.saveNetwork();
                                Console.WriteLine("DONE");
                                break;
                            }
                        case "load":
                            {
                                Console.Write("Enter name of network: ");
                                string loName = Console.ReadLine();
                                Console.Write("Loading... ");
                                try
                                {
                                    nodeClient.loadNetworkFromFile(loName);
                                    Console.WriteLine("Network loaded!");
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("{0} does not exist!", loName);
                                }
                                break;
                            }
                        case "userName":
                            {
                                Console.WriteLine("Your username is {0}", nodeClient.name);
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Unknown command!");
                                break;
                            }
                    }
                }
                }
        }
    }
}
