using System;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using CirclePrefect.Basiq;
using CirclePrefect.Util;
using System.ComponentModel;
using RemoteEmail.smtprush;
using System.Net.WebSockets;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace RemoteEmail
{
    public class Program
    {
        static Mode arg_mode;
        internal static DataStore Db;
        public static string
            arg_service,
            arg_port,
            arg_username,
            arg_password,
            arg_hostaddress,
            arg_remoteaddress,
            arg_subject,
            arg_timeout,
            arg_enablessl,
            arg_messagefile,
            arg_recipientsfile = "db",
            arg_recipient,
            arg_collectaddress,
            arg_charactername,
            arg_message;
        public static int 
            arg_tcp_port = 8080;
        static void Main(string[] _args)
        {
            for (int i = 1; i < _args.Length; i++)
            {
                switch (_args[i - 1])
                {
                    case "--tcp-port":
                        int.TryParse(_args[i], out arg_tcp_port);
                        break;
                    case "--dbname":
                        arg_recipientsfile = _args[i];
                        break;
                }
            }
            Db = new DataStore(arg_recipientsfile);
            if (!Db.BlockExists("data")) 
            {
                Db.NewBlock(new[] { "init" }, "data");
                Db.WriteToFile();
            }
            //pre-start credentials check
            //  based in TCP (see ModeHandler class) args get?
            while (true)
            { 
                string[] args = ModeHandler.GetCommands(arg_tcp_port);
                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i - 1])
                    {
                        case "-mode":
                            Enum.TryParse<Mode>(args[i], true, out arg_mode);
                            break;
                        case "-collect":
                            break;
                        case "-username":
                            arg_username = args[i];
                            break;
                        case "-service":
                            arg_service = args[i];
                            break;
                        case "-port":
                            arg_port = args[i];
                            break;
                        case "-password":
                            arg_password = args[i];
                            break;
                        case "-hostaddress":
                            arg_hostaddress = args[i];
                            break;
                        case "-remoteaddress":
                            arg_remoteaddress = args[i];
                            break;
                        case "-subject":
                            arg_subject = args[i];
                            break;
                        case "-timeout":
                            arg_timeout = args[i];
                            break;
                        case "-enablessl":
                            arg_enablessl = args[i];
                            break;
                        case "-messagefile":
                            arg_messagefile = args[i];
                            break;
                        case "-recipientsfile":
                            arg_recipientsfile = args[i];
                            break;
                        case "-recipient":
                            arg_recipient = args[i];
                            break;
                        case "--collectaddress":
                            arg_collectaddress = args[i];
                            break;
                        case "--tcp-port":
                            int.TryParse(args[i], out arg_tcp_port);
                            break;
                        case "--character":
                            arg_charactername = args[i];
                            break;
                        case "-message":
                            arg_message = args[i];
                            break;
                    }
                }
                int port = 0;
                int timeout = 0;
                bool ssl = true;
                switch (arg_mode)
                {
                    case Mode.Send:
                        int.TryParse(arg_port, out port);
                        int.TryParse(arg_timeout, out timeout);
                        bool.TryParse(arg_enablessl, out ssl);
                        Smtp.SendMail(
                            new Smtp(arg_hostaddress, 
                                new ClientInfo[] { 
                                    new ClientInfo()
                                    { 
                                        character = arg_charactername,
                                        addr = arg_collectaddress 
                                    } 
                                }, arg_subject, arg_message),
                            new SmtpInfo(arg_service, arg_username, arg_password, port, ssl, timeout)
                        );
                        break;
                    case Mode.MassSend:
                        int.TryParse(arg_port, out port);
                        int.TryParse(arg_timeout, out timeout);
                        bool.TryParse(arg_enablessl, out ssl);
                        Smtp.SendMail(
                            new Smtp(arg_hostaddress, Smtp.GetRecipients(arg_recipientsfile), arg_subject, arg_message),
                            new SmtpInfo(arg_service, arg_username, arg_password, port, ssl, timeout)
                        );
                        break;
                    case Mode.Collect:
                        Db.BlockExists("data", out Block item);
                        if (!item.HasKey(arg_charactername) && !item.HasValue(arg_collectaddress))
                        {
                            item.AddItem(arg_charactername, arg_collectaddress);
                            Db.WriteToFile();
                        }
                        //  write input data into database for use in Send and MassSend modes
                        break;
                    case Mode.Recieve:
                        //  maybe a more specific collect method?
                        break;
                }
            }
            //Environment.Exit(0);
        }
    }
}
