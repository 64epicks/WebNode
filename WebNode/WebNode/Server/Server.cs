using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WebNode.Host;
using SHttp;

namespace WebNode
{
    class Server
    {
        HttpServer httpServer;
        public Server(int port)
        {
            httpServer = new ServerBackend(port);
            Thread thread = new Thread(new ThreadStart(httpServer.listen));
            thread.Start();
        }
        public void stop()
        {
            httpServer.stop();
        }

    }
    
}
namespace WebNode.Host
{
    class ServerBackend : HttpServer
    {
        public ServerBackend(int port)
            : base(port)
        {

        }
        public void stopHost()
        {
            stop();
        }
        public override void handleGETRequest(HttpProcessor p)
        {
            throw new NotImplementedException();
        }
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            if (p.httpHeaders.Contains("Request-Type"))
            {
                p.writeFailure();
                return;
            }
            switch (p.httpHeaders["Request-Type"])
            {
                case "keyExchange":
                    {
                        
                        break;
                    }
            }
            p.writeSuccess();
        }
        public override void handleOPTIONSRequest(HttpProcessor p)
        {
            
            p.writeSuccess();
        }
    }
}
