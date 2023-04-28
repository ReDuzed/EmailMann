using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CirclePrefect.Basiq;

namespace RemoteEmail.smtprush
{
    public class SmtpInfo
    {
        ~SmtpInfo() { }
        public SmtpInfo(string service, string username, string password, int port, bool useSsl, int timeout)
        {
            this.service = service;
            this.username = username;
            this.password = password;
            this.port = port;
            this.useSsl = useSsl;
            this.timeout = timeout;
        }
        public string
            service,
            username,
            password;
        public int
            port,
            timeout;
        public bool useSsl;
    }
    public class Smtp
    {
        private string
            from,
            subject,
            content;
        private ClientInfo[] recipients;
        ~Smtp() { }
        internal Smtp(string from, ClientInfo[] recipients, string subject, string content)
        {
            this.content = content.Trim('"');
            this.from = from;
            this.subject = subject;
            this.recipients = recipients;
        }
        public static void SendMail(Smtp smtp, SmtpInfo info)
        {
            if (info.timeout < 6000) info.timeout = 6000;
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                SmtpClient _smtp = new SmtpClient
                {
                    Host = info.service,
                    Port = info.port,
                    EnableSsl = info.useSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(info.username, info.password),
                    Timeout = info.timeout
                };

                for (int i = 0; i < smtp.recipients.Length; i++)
                {
                    try
                    {
                        MailMessage message = new MailMessage(smtp.from, smtp.recipients[i].addr, smtp.subject, smtp.content);
                        message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                        _smtp.Send(message);
                    }
                    catch (Exception ex)
                    {
                        WarningMessage(ex);
                    }
                }
                _smtp.Dispose();
            }
            catch (Exception ex)
            {
                WarningMessage(ex);
            }
        }
        private static void WarningMessage(Exception ex)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("{0} Exception caught.", ex.Message);
            Console.ForegroundColor = color;
        }
        internal static ClientInfo[] GetRecipients(string path)
        {
            // Read database
            // Change: add
            Block item = Program.Db.GetBlock("data");
            ClientInfo[] array = new ClientInfo[item.Contents.Length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new ClientInfo() 
                { 
                    character = item.Keys()[i],
                    addr = item.Values()[i]
                };
            }
            return array;
        }
    }
    internal struct ClientInfo
    {
        public string addr;
        public string character;
    }
}
