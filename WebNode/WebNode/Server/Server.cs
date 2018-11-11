using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WebNode.Host;
using SHttp;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Net;
using System.Collections;

namespace WebNode
{
    class Server
    {
        Host.Server httpServer;
        public Server(int port, KeyProduct keyProduct)
        {
            httpServer = new ServerBackend(port, keyProduct);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
        }
        public void stop()
        {
            httpServer.isActive = false;
        }

    }
    
}
namespace WebNode.Host
{
    class ServerBackend : Server
    {
        Dictionary<string, string> keys = new Dictionary<string, string>();
        KeyProduct keyProduct;

        public ServerBackend(int port, KeyProduct keyProduct)
            : base(port)
        {
            this.keyProduct = keyProduct;
        }
        public void stopHost()
        {
            isActive = false;
        }
        public override void handleGETRequest(Processor p)
        {
            p.writeFailure(200, "OK");
        }
        public override void handlePOSTRequest(Processor p, StreamReader inputData)
        {
            string data = inputData.ReadToEnd();
            if (!p.httpHeaders.Contains("Request-Type"))
            {
                p.outputStream.WriteLine("HTTP/1.1 400 Bad Request");
                p.outputStream.WriteLine("Connection: close");
                p.outputStream.WriteLine("");
                return;
            }
            switch (p.httpHeaders["Request-Type"])
            {
                case "keyExchange":
                    {
                        try
                        {
                            var keyInfo = JsonConvert.DeserializeObject<KeyProduct>(data);
                            var rsa = new RSACryptoServiceProvider();
                            rsa.FromXmlString(keyInfo.key);
                            keys.Add(keyInfo.cookie, keyInfo.key);
                        } catch (Exception)
                        {
                            p.outputStream.WriteLine("HTTP/1.1 400 Bad Request");
                            p.outputStream.WriteLine("Connection: close");
                            p.outputStream.WriteLine("");
                            p.outputStream.WriteLine("Couldn't read json");
                            return;
                        }
                        

                        var keySend = new KeyProduct();

                        keySend.key = keyProduct.cookie;

                        p.writeSuccess();

                        p.outputStream.WriteLine(JsonConvert.SerializeObject(keySend));

                        break;
                    }
            }
        }
        public override void handleOPTIONSRequest(Processor p)
        {
            
            p.writeSuccess();
        }
    }
    public abstract class Server
    {
        protected int port;
        private TcpClient _socket;
        private TcpListener _listener;
        public bool isActive = true;

        public Server(int port)
        {
            this.port = port;
        }
        public void listen()
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();
            while (isActive)
            {
                _socket = _listener.AcceptTcpClient();
                Processor processor = new Processor(_socket, this);
                Thread thread = new Thread(processor.process);
                thread.Start();
                Thread.Sleep(1);
            }
        }

        public abstract void handleGETRequest(Processor p);
        public abstract void handlePOSTRequest(Processor p, StreamReader inputData);
        public abstract void handleOPTIONSRequest(Processor p);
    }
    public class Processor
    {
        public TcpClient socket;
        public Server server;

        public Stream inputStream;
        public StreamWriter outputStream;
        public string http_method;
        public string http_url;
        public string protocol;
        public Hashtable httpHeaders = new Hashtable();
        public string endPoint;
        public StreamReader body;

        public Processor(TcpClient tcpClient, Server server)
        {
            this.socket = tcpClient;
            this.server = server;
        }
        public void process()
        {
            //byte[] data = new byte[4096];
            //var inputStreamVis = socket.GetStream();
            inputStream = new BufferedStream(socket.GetStream());
            outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
            try
            {
                //int reader = inputStreamVis.Read(data, 0, data.Length);
                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, reader));
                parse();
                readHeaders();
                handle();
            }
            catch (Exception e)
            {
                if (e.Message == "Header does not exist!") writeFailure(400, "Bad Request");
                else writeFailure(500, "Internal Server Error");
                Console.WriteLine(e.Message + e.StackTrace);
            }
            outputStream.Flush();
            inputStream = null; outputStream = null;
            socket.Close();
        }
        public void handle()
        {
            switch (http_method)
            {
                case "GET":
                    {
                        server.handleGETRequest(this);
                        break;
                    }
                case "POST":
                    {
                        Console.WriteLine("get post data start");
                        int content_len = 0;
                        MemoryStream ms = new MemoryStream();
                        if (this.httpHeaders.ContainsKey("Content-Length"))
                        {
                            content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                            byte[] buf = new byte[BUF_SIZE];
                            int to_read = content_len;
                            while (to_read > 0)
                            {
                                Console.WriteLine("starting Read, to_read={0}", to_read);

                                int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                                Console.WriteLine("read finished, numread={0}", numread);
                                if (numread == 0)
                                {
                                    if (to_read == 0)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        throw new Exception("client disconnected during post");
                                    }
                                }
                                to_read -= numread;
                                ms.Write(buf, 0, numread);
                            }
                            ms.Seek(0, SeekOrigin.Begin);
                        }
                        Console.WriteLine("get post data end");
                        server.handlePOSTRequest(this, new StreamReader(ms));

                        break;
                    }
                case "OPTIONS":
                    {
                        server.handleOPTIONSRequest(this);
                        break;
                    }
            }
        }
        private const int BUF_SIZE = 4096;
        public void readBody(string[] headers)
        {
            // this post data processing just reads everything into a memory stream.
            // this is fine for smallish things, but for large stuff we should really
            // hand an input stream to the request processor. However, the input stream 
            // we hand him needs to let him see the "end of the stream" at this content 
            // length, because otherwise he won't know when he's seen it all! 

            //Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (headerExist("Content-Length", headers))
            {
                content_len = Convert.ToInt32(findHeader("Content-Length", headers));
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            body = new StreamReader(ms);

        }
        public void writeBack(string[] response)
        {
            foreach (string res in response)
            {
                outputStream.WriteLine(res);
            }
        }
        public void writeFailure(int code, string message)
        {
            outputStream.WriteLine("HTTP/1.0 {0} {1}", code, message);
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
        public void writeSuccess(string content_type = "text/html")
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: " + content_type);
            outputStream.WriteLine("Access-Control-Allow-Origin:*");
            outputStream.WriteLine("Access-Control-Allow-Headers:content-type,request-type");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
        private void parse()
        {
            string[] first = streamReadLine(inputStream).Split(' ');
            http_method = first[0].ToUpper();
            http_url = first[1];
            protocol = first[2];
        }
        public void readHeaders()
        {
            string line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                string name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;
                }

                string value = line.Substring(pos, line.Length - pos);
                Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value;
            }
        }
        public bool headerExist(string headerName, string[] headers)
        {
            foreach (string head in headers)
            {
                if (head == headerName) return true;
            }
            return false;
        }
        public string findHeader(string headerName, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++) if (headers[i].Split(':')[0] == headerName) return headers[i].Split(':')[1].Substring(1, headers[i].Split(':')[1].Length - 1);
            throw new Exception("Header does not exist!");
        }
        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
    }
    class KeyProduct
    {
        public string cookie { get; set; }
        public string key { get; set; }
    }
}
