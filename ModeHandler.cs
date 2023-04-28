using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RemoteEmail
{
    public class ModeHandler
    {
        static bool init = false;
        static TcpListener listen;
        static TcpClient tcp;
        static ManualResetEvent wait = new ManualResetEvent(false);
        ~ModeHandler() { }
        static void Init(int port)
        {
            listen = new TcpListener(IPAddress.Any, port);
            listen.Start();
        }
        public static string[] GetCommands(int port)
        {
            if (!init)
            {
                Init(port);
                init = true;
            }
            tcp = listen.AcceptTcpClient();
            AwaitData();
            WaitHandle.WaitAll(new[] { wait });
            var net = tcp.GetStream();
            int read;
            string text = "";
            byte[] buffer = new byte[4096];
            while ((read = net.Read(buffer, 0, buffer.Length)) > 0)
            {
                text += Encoding.ASCII.GetString(buffer, 0, read);
            }
            text = text.TrimEnd(' ');
            return ConvertToArgs(text);
        }
        static string[] ConvertToArgs(string text)
        {
            IList<string> list = new List<string>();
            string cmd = "";
            int length = 0;
            bool flag = false;
            for (int i = 0; i < text.Length; i++)
            {
                HASARG:
                if (flag)
                {
                    if (text[i] == '"')
                    {
                        length = text.LastIndexOf('"') - i + 1;
                        list.Add(text.Substring(i, length));
                        i += length + 1;
                        flag = false;
                        continue;
                    }
                    length = text.Substring(i).IndexOf(' ');
                    if (length == -1)
                    {
                        length = text.Length - i;
                    }
                    cmd = text.Substring(i, length).TrimEnd(' ');
                    i += length;
                    flag = false;
                    list.Add(cmd);
                    continue;
                }
                if (text[i] != ' ' && !flag)
                {
                    flag = true;
                    goto HASARG;
                }
            }
            return list.ToArray();
        }
        static void AwaitData()
        {
            Task.Factory.StartNew(async () =>
            {
                wait.Reset();
                while (tcp.Available == 0)
                {
                    await Task.Delay(3000);
                }
                wait.Set();
            });
        }
    }

}
