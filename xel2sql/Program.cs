//
//This Sample Code is provided for the purpose of illustration only and is not intended to be used in a production environment.  
//THIS SAMPLE CODE AND ANY RELATED INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.  
//We grant You a nonexclusive, royalty-free right to use and modify the 
//Sample Code and to reproduce and distribute the object code form of the Sample Code, provided that You agree: (i) to not use Our name, logo, or trademarks to market Your software product in which the Sample Code is embedded; 
//(ii) to include a valid copyright notice on Your software product in which the Sample Code is 
//embedded; and 
//(iii) to indemnify, hold harmless, and defend Us and Our suppliers from and against any claims or lawsuits, including attorneys’ fees, that arise or result from the use or distribution of the Sample Code.
//Please note: None of the conditions outlined in the disclaimer above will supercede the terms and conditions contained within the Premier Customer Services Description.
//
using Microsoft.SqlServer.XEvent.Linq;
using System;
using System.Linq;
using System.IO;
using System.Text;

namespace xel2sql
{
    class Program
    {
        static string inputFileSpec = "";
        static string outputFileSpec = "";

        static void Main(string[] args)
        {
            if (ProcessCommandLine(args))
            {
                DateTime start = DateTime.Now;
                CreateOutput();
                DateTime finish = DateTime.Now;
                Console.WriteLine("xel2sql completed in : {0} seconds", finish-start);
                Console.ReadLine();
            }
        }
        static bool ProcessCommandLine(string[] args)
        {
            bool argsValid = true;
            //validate input arguements
            if (args.Count() < 4
                || args[0] != "/i"
                || args[2] != "/o")
            {
                Console.WriteLine("usage:\r\n\txel2sql\r\n /i filespec /o output.sql");
                Console.ReadLine();
                argsValid = false;
            }
            else
            {
                inputFileSpec = args[1];
                outputFileSpec = args[3];
            }
            return argsValid;
        }
        static void CreateOutput()
        {
            QueryableXEventData events =
                new QueryableXEventData(inputFileSpec);
            using (FileStream f = File.Create(outputFileSpec))
            {
                foreach (PublishedEvent evt in events)
                {
                    if (evt.Name == "rpc_starting"
                        || evt.Name == "sql_batch_starting"
                        || evt.Name == "login"
                        || evt.Name == "logout")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($"-- {evt.Name}\r\n");
                        sb.Append($"-- Session id: {evt.Actions["session_id"].Value}\t\t\t");
                        sb.Append($"-- Sequence #: {evt.Actions["event_sequence"].Value}\r\n");
                        sb.Append($"-- Client Hostname {evt.Actions["client_hostname"].Value}\t");
                        sb.Append($"-- Username {evt.Actions["nt_username"].Value}\r\n\r\n");

                        if (evt.Name == "rpc_starting")
                        {
                            sb.Append(evt.Fields["statement"].Value + "\r\n");
                            sb.Append("GO \r\n");
                        }
                        if (evt.Name == "sql_batch_starting")
                        {
                            sb.Append(evt.Fields["batch_text"].Value + "\r\n");
                            sb.Append("GO \r\n\r\n");
                        }
                        if (evt.Name == "login")
                        {
                            sb.Append("-- Connection Options for this login\r\n");
                            sb.Append(evt.Fields["options_text"].Value + "\r\n");
                        }
                        sb.Append("\r\n");
                        Byte[] info = new UTF8Encoding(false).GetBytes(sb.ToString());
                        f.Write(info, 0, info.Length);
                    }
                }
            }
        }
    }
}
