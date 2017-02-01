/////////////////////////////////////////////////////////////////////
//	Provides functionality to search for text strings
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
 *  Package Contents:
 *  -----------------
 *  This package defines four classes:
 *  Server:
 *    Provides prototype behavior for the DocumentVault server.
 *  EchoCommunicator:
 *    Simply diplays its messages on the server Console.
 *  QueryCommunicator:
 *    Serves as a placeholder for query processing.  You should be able to
 *    invoke your Project #2 query processing from the ProcessMessages function.
 *  NavigationCommunicator:
 *    Serves as a placeholder for navigation processing.  You should be able to
 *    invoke your navigation processing from the ProcessMessages function.
 * 
 *  Required Files:
 *  - Server:      Server.cs, Sender.cs, Receiver.cs
 *  - Components:  ICommLib, AbstractCommunicator, BlockingQueue
 *  - CommService: ICommService, CommService
 *
 *  Required References:
 *  - System.ServiceModel
 *  - System.RuntimeSerialization
 *
 *  Build Command:  devenv Project4HelpF13.sln /rebuild debug
 *
 *  Maintenace History:
 *  ver 2.1 : Nov 7, 2013
 *  - replaced ServerSender with a merged Sender class
 *  ver 2.0 : Nov 5, 2013
 *  - fixed bugs in the message routing process
 *  ver 1.0 : Oct 29, 2013
 *  - first release
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string ServerUrl = "http://localhost:8000/CommService";
        public static string ClientUrl = "http://localhost:8001/CommService";

        internal static Sender clientSender;
        internal static Receiver clientReceiver;

        internal static ClientFileReceiver clientFileReceiver;

        // initiates several handlers on application start
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            clientSender = new Sender();
            clientSender.Name = "ClientSender";
            clientSender.Connect(ServerUrl);
            clientSender.Start();

            clientReceiver = new Receiver(ClientUrl);

            clientFileReceiver = new ClientFileReceiver();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ClientFileReceiver.FilePath = ServerFileReceiver.FilePath = Directory.GetParent(loc).Parent.FullName + @"\VaultFiles";
            clientFileReceiver.PrepareToReceiveFile("http://localhost:8090/ClientFileService");
        }

        // closes several handlers on application end
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            clientSender.Stop();
            clientSender.Wait();

            foreach (var item in clientReceiver.VaultDispatcher.CommunicatorLookup.Keys)
            {
                var ICommService = clientReceiver.VaultDispatcher.CommunicatorLookup[item] as AbstractCommunicator;
                if (ICommService.Name != "ClientSender")
                {
                    ICommService.Stop();
                    ICommService.Wait();
                }
            }
            clientReceiver.Stop();
            clientReceiver.Close();
        }
    }
}
