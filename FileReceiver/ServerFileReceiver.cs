///////////////////////////////////////////////////////////////////////
// FileService.cs - Self-hosted file transfer service                //
//                                                                   //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2010   //
///////////////////////////////////////////////////////////////////////

/*
 * You need to run both FileService and Client with administrator 
 * priviledges.  You do that by running Visual Studio as administrator
 * or right-clicking on the FileService.exe and Client.exe and selecting
 * run as administrator.
 * This service is configured with WSHttpBinding.
 * - That has the advantage that messages, when sent across the network
 *   by default arrive in the order sent.
 * - It has the disadvantage that machines that use or host the service
 *   need to have digital certificates installed.
 * - Visual Studio 2010 installs the requireded certificate,
 */

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;
using System.IO;

namespace RDV
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ServerFileReceiver : IFileService
    {
        static string filePath = string.Empty;
        string fileSpec = string.Empty;
        FileStream fileStream = null;  // remove static for WSHttpBinding

        public static string FilePath { get { return filePath; } set { if (filePath != value) filePath = value; } }

        public bool OpenFileForWrite(string name)
        {
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);

            fileSpec = filePath + "\\" + name;
            try
            {
                fileStream = File.Open(fileSpec, FileMode.Create, FileAccess.Write);
                Console.Write("\n  {0} opened", fileSpec);
                return true;
            }
            catch
            {
                Console.Write("\n  {0} failed to open", fileSpec);
                return false;
            }
        }

        public bool WriteFileBlock(byte[] block)
        {
            try
            {
                Console.Write("\n  writing block with {0} bytes", block.Length);
                fileStream.Write(block, 0, block.Length);
                fileStream.Flush();
                return true;
            }
            catch { return false; }
        }

        public bool CloseFile()
        {
            try
            {
                fileStream.Close();
                Console.Write("\n  {0} closed", fileSpec);
                return true;
            }
            catch { return false; }
        }

        ServiceHost CreateChannel(string url)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(url);
            Type service = typeof(RDV.ServerFileReceiver);
            ServiceHost host = new ServiceHost(service, baseAddress);
            host.AddServiceEndpoint(typeof(RDV.IFileService), binding, baseAddress);
            return host;
        }

        //public void PrepareToReceiveFile()
        //{
        //    Console.Write("\n  Starting File Transfer Service");
        //    Console.Write("\n  ======================\n");

        //    ServiceHost fs = CreateChannel("http://localhost:8080/FileService");
        //    fs.Open();

        //    Console.Write("\n  Started File Transfer Service\n ");
        //}

        public void PrepareToReceiveFile(string url)
        {
            Console.Write("\n  Starting File Transfer Service");
            Console.Write("\n  ======================\n");

            ServiceHost fs = CreateChannel(url);
            fs.Open();

            Console.Write("\n  Started File Transfer Service\n ");
        }

        static void Main(string[] args)
        {
            ServerFileReceiver fileReceiver = new ServerFileReceiver();

            Console.Write("\n  File Transfer Service running:");
            Console.Write("\n ================================\n");

            ServiceHost host = fileReceiver.CreateChannel("http://localhost:8080/FileService");
            host.Open();
            Console.Write("\n  Press key to terminate service:\n");
            Console.ReadKey();
            Console.Write("\n");
            host.Close();
        }
    }
}
