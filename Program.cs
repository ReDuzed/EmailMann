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
            if (_args.Length == 1)
            {
                switch (_args[0])
                {
                    case "/?":
                    case "/help":
                    case "--help":
                    case "-help":
                        Console.WriteLine("Written by Reduzed, 2023\n\n" +
                            "Argument           Input           Information\n" +
                            "--tcp-port         #               Port to listen on for incoming arguments\n" +
                            "--dbname           text            Name of database (currently BasiqDB)\n" +
                            "-mode              enum            Name of desired mode (Send, MassSend)\n" +
                            "-collect           text            unused\n" +
                            "-username          text            Username for SMTP service\n" +
                            "-service           text            URI/URL of SMTP service\n" +
                            "-port              #               Port of SMTP service\n" +
                            "-password          text            Password for SMTP service\n" +
                            "-hostaddress       text            E-mail from which message is being sent\n" +
                            "-remoteaddress     text            unused\n" +
                            "-subject           text            Subject of E-mail\n" +
                            "-timeout           #               unused (is always set to 60000)\n" +
                            "-enablessl         bool            Whether SSL is enabled or not\n" +
                            "-messagefile       URI             unused" +
                            "-recipientsfile    URI             Database name (currently BasiqDB)\n" +
                            "-recipient         text            unused\n" +
                            "-message           text            E-mail message content, enclose in double quotations\n" +
                            "--collectaddress   text            For sending an individual an E-mail\n" +
                            "\nIncoming arguments through a .NET TCP listener\n" +
                            "Argument           Input           Information\n" +
                            "--collectaddress   text            For the mailing list subscription\n" +
                            "--character        text            E-mail subscription alias\n" +
                            "-mode              enum            Name of desired mode (Collect)\n");
                    break;
                }
            } 
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
                    case "-mode":
                        Enum.TryParse<Mode>(_args[i], true, out arg_mode);
                        break;
                    case "-collect":
                        break;
                    case "-username":
                        arg_username = _args[i];
                        break;
                    case "-service":
                        arg_service = _args[i];
                        break;
                    case "-port":
                        arg_port = _args[i];
                        break;
                    case "-password":
                        arg_password = _args[i];
                        break;
                    case "-hostaddress":
                        arg_hostaddress = _args[i];
                        break;
                    case "-remoteaddress":
                        arg_remoteaddress = _args[i];
                        break;
                    case "-subject":
                        arg_subject = _args[i];
                        break;
                    case "-timeout":
                        arg_timeout = _args[i];
                        break;
                    case "-enablessl":
                        arg_enablessl = _args[i];
                        break;
                    case "-messagefile":
                        arg_messagefile = _args[i];
                        break;
                    case "-recipientsfile":
                        arg_recipientsfile = _args[i];
                        break;
                    case "-recipient":
                        arg_recipient = _args[i];
                        break;
                    case "-message":
                        arg_message = _args[i];
                        break;
                    case "--collectaddress":
                        arg_collectaddress = _args[i];
                        break;
                    case "--character":
                        arg_charactername = _args[i];
                        break;
                }
            }
            Db = new DataStore(arg_recipientsfile);
            if (!Db.BlockExists("data")) 
            {
                Db.NewBlock(new[] { "init" }, "data");
                Db.WriteToFile();
            }
            int port = 0;
            int timeout = 0;
            bool ssl = true;
            switch (arg_mode)
            {
                case Mode.Send:
                    int.TryParse(arg_port, out port);
                    int.TryParse(arg_timeout, out timeout);
                    if (arg_enablessl != null)
                    {
                        bool.TryParse(arg_enablessl, out ssl);
                    }
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
                    WriteLine($"Recipient sent:\n   {arg_message}");
                    return;
                case Mode.MassSend:
                    int.TryParse(arg_port, out port);
                    int.TryParse(arg_timeout, out timeout);
                    bool.TryParse(arg_enablessl, out ssl);
                    var list = Smtp.GetRecipients(arg_recipientsfile);
                    Smtp.SendMail(
                        new Smtp(arg_hostaddress, list, arg_subject, arg_message),
                        new SmtpInfo(arg_service, arg_username, arg_password, port, ssl, timeout)
                    );
                    WriteLine(list.Length + $" recipients sent:\n   {arg_message}");
                    return;
            }
            //pre-start credentials check
            //  based in TCP (see ModeHandler class) args get?
            while (true)
            { 
                //  Needs Regex sanity checks
                string[] args = ModeHandler.GetCommands(arg_tcp_port);
                for (int i = 1; i < args.Length; i++)
                {
                    switch (args[i - 1])
                    {
                        case "--collectaddress":
                            arg_collectaddress = args[i];
                            break;
                        case "--character":
                            arg_charactername = args[i];
                            break;
                        case "-mode":
                            Enum.TryParse<Mode>(_args[i], true, out arg_mode);
                            break;
                    }
                }
                switch (arg_mode)
                {
                    case Mode.Collect:
                        //  Use a more substantial database, sql?
                        Db.BlockExists("data", out Block item);
                        if (!item.HasKey(arg_charactername) && !item.HasValue(arg_collectaddress))
                        {
                            item.AddItem(arg_charactername, arg_collectaddress);
                            Db.WriteToFile();
                            WriteLine("Collected");
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
        internal static void WriteLine(string text, ConsoleColor color = ConsoleColor.Gray, TimeSpan timeout = default)
        {
            var _color = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = _color;
            Task.Delay(timeout);
        }
    }
}
