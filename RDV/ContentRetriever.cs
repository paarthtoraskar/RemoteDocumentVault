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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using CTA;
using System.Windows;

namespace RDV
{
    /// <summary>
    /// Retrieves text data content and metadata properties for a specified file
    /// </summary>
    class ClientContentRetrieverCommunicator : AbstractCommunicator
    {
        public Action<string, MetadataProperty> UpdateUI;

        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                {
                    if (Verbose)
                        Console.Write("\n  ClientContentRetrieverCommunicator shutting down");

                    // shut down dispatcher

                    msg.TargetCommunicator = "dispatcher";
                    AbstractMessageDispatcher.GetInstance().PostMessage(msg);
                    break;
                }
                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            //string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //List<string> twoFiles = Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\VaultFiles", msg.Contents.ToString() + ".*").ToList();
            //List<string> twoFiles = Directory.GetFiles(ClientFileReceiver.FilePath, Path.GetFileNameWithoutExtension(msg.Contents.ToString()) + ".*").ToList();
            List<string> twoFiles = new List<string>();
            twoFiles.Add(Directory.GetFiles(ClientFileReceiver.FilePath, msg.Contents.ToString()).Single());
            twoFiles.Add(Directory.GetFiles(ClientFileReceiver.FilePath, Path.GetFileNameWithoutExtension(msg.Contents.ToString()) + ".xml").Single());
            string xmlFile = string.Empty;
            string actualFile = string.Empty;
            foreach (var item in twoFiles)
            {
                if (Path.GetExtension(item) == ".xml")
                    xmlFile = item;
                else
                    actualFile = item;
            }

            string allTextOfActualFile = File.ReadAllText(actualFile);
            MetadataProperty readProps = readAllProps(xmlFile);

            File.Delete(xmlFile);
            File.Delete(actualFile);

            if (Application.Current.Dispatcher.CheckAccess())
                UpdateUI(allTextOfActualFile, readProps);
            else
                Application.Current.Dispatcher.Invoke(
                  UpdateUI,
                  System.Windows.Threading.DispatcherPriority.Background,
                  new object[] { allTextOfActualFile, readProps }
                );
        }

        // reads all metadata properties from the given xml file
        MetadataProperty readAllProps(string xmlFile)
        {
            XDocument xmlFileContent = XDocument.Load(xmlFile);
            MetadataProperty readProps = new MetadataProperty();
            readProps.Name = xmlFileContent.Descendants("Name").Single().Value;
            readProps.LastModified = DateTime.Parse(xmlFileContent.Descendants("LastModified").Single().Value);
            readProps.Version = xmlFileContent.Descendants("Version").Single().Value;
            readProps.Description = xmlFileContent.Descendants("Description").Single().Value;
            readProps.Dependencies = new List<string>();
            var dependancies = xmlFileContent.Descendants("Dependancy").ToList();
            foreach (var dependancy in dependancies)
                readProps.Dependencies.Add(dependancy.Value.ToString());
            readProps.Keywords = new List<string>();
            var keywords = xmlFileContent.Descendants("Keyword").ToList();
            foreach (var keyword in keywords)
                readProps.Keywords.Add(keyword.Value.ToString());
            readProps.Categories = new List<string>();
            var categories = xmlFileContent.Descendants("Category").ToList();
            foreach (var category in categories)
                readProps.Categories.Add(category.Value.ToString());

            return readProps;
        }
    }
}
