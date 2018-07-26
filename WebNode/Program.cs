using System;
using System.IO;
using SharpDB;

namespace WebNode
{
    public class Program
    {
		static DB dB = new DB(Environment.CurrentDirectory);
		static void Main(string[] args){
			if (!Directory.Exists("db")) {
				Console.WriteLine("No connection found");
			}
		}
		static void Connect(){
			
		}
    }
}
