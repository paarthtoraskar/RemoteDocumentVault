///////////////////////////////////////////////////////////////////////////////
// Server.cs - Document Vault Server prototype                               //
//                                                                           //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2013           //
///////////////////////////////////////////////////////////////////////////////

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

/////////////////////////////////////////////////////////////////////
//	Provides functionality to search for text strings
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Xml.Linq;

using CTA;
using Navig;
using System.Xml;
using System.IO;
using System.Reflection;

namespace RDV
{
    class Server
    {
        static void Main(string[] args)
        {
            string ServerUrl = "http://localhost:8000/CommService";
            Receiver receiver = new Receiver(ServerUrl);
            string ClientUrl = "http://localhost:8001/CommService";
            Sender sender = new Sender();
            sender.Name = "ServerSender";
            sender.Connect(ClientUrl);
            receiver.Register(sender);
            sender.Start();

            ServerTextFinderCommunicator serverTextFinderCommunicator = new ServerTextFinderCommunicator();
            serverTextFinderCommunicator.Name = "ServerTextFinderCommunicator";
            receiver.Register(serverTextFinderCommunicator);
            serverTextFinderCommunicator.Start();

            ServerMetadataRetrieverCommunicator serverMetadataRetrieverCommunicator = new ServerMetadataRetrieverCommunicator();
            serverMetadataRetrieverCommunicator.Name = "ServerMetadataRetrieverCommunicator";
            receiver.Register(serverMetadataRetrieverCommunicator);
            serverMetadataRetrieverCommunicator.Start();

            ServerFileReceiver serverFileReceiver = new ServerFileReceiver();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ServerFileReceiver.FilePath = Directory.GetParent(loc).Parent.FullName + @"\Vault";
            serverFileReceiver.PrepareToReceiveFile("http://localhost:8080/ServerFileService");

            ServerCategoriesRetrieverCommunicator serverCategoriesRetrieverCommunicator = new ServerCategoriesRetrieverCommunicator();
            serverCategoriesRetrieverCommunicator.Name = "ServerCategoriesRetrieverCommunicator";
            receiver.Register(serverCategoriesRetrieverCommunicator);
            serverCategoriesRetrieverCommunicator.Start();

            ServerFilesRetrieverCommunicator serverFilesRetrieverCommunicator = new ServerFilesRetrieverCommunicator();
            serverFilesRetrieverCommunicator.Name = "ServerFilesRetrieverCommunicator";
            receiver.Register(serverFilesRetrieverCommunicator);
            serverFilesRetrieverCommunicator.Start();

            ServerContentRetrieverCommunicator serverContentRetrieverCommunicator = new ServerContentRetrieverCommunicator();
            serverContentRetrieverCommunicator.Name = "ServerContentRetrieverCommunicator";
            receiver.Register(serverContentRetrieverCommunicator);
            serverContentRetrieverCommunicator.Start();

            ServerFileExtractorCommunicator serverFileExtractorCommunicator = new ServerFileExtractorCommunicator();
            serverFileExtractorCommunicator.Name = "ServerFileExtractorCommunicator";
            receiver.Register(serverFileExtractorCommunicator);
            serverFileExtractorCommunicator.Start();
        }
    }

    /// <summary>
    /// Retrieves categories that span across all files in the vault
    /// </summary>
    class ServerCategoriesRetrieverCommunicator : AbstractCommunicator
    {
        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            ServiceMessage replyMsg = zipCategoriesRetrieverServerMessage(msg);
            AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            dispatcher.PostMessage(replyMsg);
        }

        // creates the server reply message for this communicator
        ServiceMessage zipCategoriesRetrieverServerMessage(ServiceMessage msg)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("CategoriesRetriever");
                xmlWriter.WriteStartElement("Categories");
                foreach (var cat in getCategories())
                    xmlWriter.WriteElementString("Category", cat);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, xmlDoc.OuterXml);
            replyMsg.TargetUrl = msg.SourceUrl;
            replyMsg.SourceUrl = msg.TargetUrl;
            return replyMsg;
        }

        // gets categories that span across all files in the vault
        List<string> getCategories()
        {
            List<string> categories = new List<string>();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            List<string> xmlfiles = Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.xml").ToList();

            foreach (var item in xmlfiles)
            {
                var xmlDoc = XDocument.Load(item);
                var categoriesXElements = xmlDoc.Descendants("Category").ToList();
                foreach (var category in categoriesXElements)
                {
                    if (!categories.Contains(category.Value))
                        categories.Add(category.Value);
                }
            }
            return categories;
        }
    }

    /// <summary>
    /// Finds text strings in files in the vault that belong to a specified categories
    /// and returns those file names
    /// </summary>
    class ServerTextFinderCommunicator : AbstractCommunicator
    {
        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            string allOrOne = string.Empty;
            List<string> textStrings = new List<string>();
            List<string> selectedCategories = new List<string>();
            List<string> filesForCategories = new List<string>();

            unzipTextFinderClientMessage(msg, ref allOrOne, ref textStrings, ref selectedCategories);

            filesForCategories = FilesForCategories.getFilesForCategories(selectedCategories, true);

            TextFinder textFinder = null;
            if (allOrOne == "All")
                textFinder = new TextFinder(TypeOfSearch.All);
            else
                textFinder = new TextFinder(TypeOfSearch.AtleastOne);
            textFinder.DetermineTextStringsToSearch(textStrings);

            Navigate nav = new Navigate(false);
            nav.newFile += new Navigate.newFileHandler(textFinder.FindText);

            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //if (filesForCategories.Count == 0)
            //    nav.Go(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.*");
            //else
            nav.Go(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.*", filesForCategories);

            ServiceMessage replyMsg = zipTextFinderServerMessage(textFinder.FileNames, msg);
            AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            dispatcher.PostMessage(replyMsg);
        }

        // disassebles the message sent by the client for this communicator
        void unzipTextFinderClientMessage(ServiceMessage msg, ref string allOrOne, ref List<string> textStrings, ref List<string> selectedCategories)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());

            allOrOne = (from item in xDoc.Descendants() where item.Name == "AllOrOne" select item).FirstOrDefault().Value;
            var ts = (from item in xDoc.Descendants() where item.Name == "TextString" select item).ToList();
            foreach (var item in ts)
                textStrings.Add(item.Value);
            var sc = (from item in xDoc.Descendants() where item.Name == "Category" select item).ToList();
            foreach (var item in sc)
                selectedCategories.Add(item.Value);
        }

        // creates the server reply message for this communicator
        ServiceMessage zipTextFinderServerMessage(List<string> fileNames, ServiceMessage msg)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("TextFinder");
                xmlWriter.WriteStartElement("FileNames");
                foreach (var item in fileNames)
                    xmlWriter.WriteElementString("FileName", Path.GetFileName(item));
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, xmlDoc.OuterXml);
            replyMsg.TargetUrl = msg.SourceUrl;
            replyMsg.SourceUrl = msg.TargetUrl;

            return replyMsg;
        }
    }

    /// <summary>
    /// Retrieves specified metadata properties for all files
    /// in the vault that belong to a specified categories
    /// </summary>
    class ServerMetadataRetrieverCommunicator : AbstractCommunicator
    {
        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            List<string> metaPropsToRetrieve = new List<string>();
            List<string> selectedCategories = new List<string>();
            List<string> filesForCategories = new List<string>();

            unzipMetadataRetrieverClientMessage(msg, ref metaPropsToRetrieve, ref selectedCategories);

            filesForCategories = FilesForCategories.getFilesForCategories(selectedCategories, true);

            MetadataRetriever metadataRetr = new MetadataRetriever();
            metadataRetr.DetermineMetadataPropsToRetrieve(metaPropsToRetrieve);

            Navigate nav = new Navigate(false);
            nav.newFile += new Navigate.newFileHandler(metadataRetr.RetrieveMetadata);

            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //if (filesForCategories.Count == 0)
            //    nav.Go(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.*");
            //else
            nav.Go(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.*", filesForCategories);

            ServiceMessage replyMsg = zipMetadataRetrieverServerMessage(metadataRetr.RetrievedMetadata, msg);
            AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            dispatcher.PostMessage(replyMsg);
        }

        // disassebles the message sent by the client for this communicator
        void unzipMetadataRetrieverClientMessage(ServiceMessage msg, ref List<string> metaPropsToRetrieve, ref List<string> selectedCategories)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());

            var mp = (from item in xDoc.Descendants() where item.Name == "MetaProp" select item).ToList();
            foreach (var item in mp)
                metaPropsToRetrieve.Add(item.Value);
            var sc = (from item in xDoc.Descendants() where item.Name == "Category" select item).ToList();
            foreach (var item in sc)
                selectedCategories.Add(item.Value);
        }

        // creates the server reply message for this communicator
        ServiceMessage zipMetadataRetrieverServerMessage(List<KeyValuePair<string, MetadataProperty>> retrievedMetadata, ServiceMessage msg)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("MetadataRetriever");
                xmlWriter.WriteStartElement("Metadata");
                foreach (var item in retrievedMetadata)
                {
                    xmlWriter.WriteStartElement("Metadatum");
                    xmlWriter.WriteElementString("FullName", item.Key);
                    xmlWriter.WriteElementString("Name", item.Value.Name);
                    xmlWriter.WriteElementString("LastModified", item.Value.LastModified.ToString());
                    xmlWriter.WriteElementString("Version", item.Value.Version.ToString());
                    xmlWriter.WriteElementString("Description", item.Value.Description);
                    xmlWriter.WriteStartElement("Dependancies");
                    foreach (var dep in item.Value.Dependencies)
                        xmlWriter.WriteElementString("Dependancy", dep);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteStartElement("Keywords");
                    foreach (var kw in item.Value.Keywords)
                        xmlWriter.WriteElementString("Keyword", kw);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, xmlDoc.OuterXml);
            replyMsg.TargetUrl = msg.SourceUrl;
            replyMsg.SourceUrl = msg.TargetUrl;
            return replyMsg;
        }
    }

    /// <summary>
    /// Retrieves files that belong to a specified categories
    /// </summary>
    class ServerFilesRetrieverCommunicator : AbstractCommunicator
    {
        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            List<string> selectedCategories = new List<string>();
            unzipFilesRetrieverClientMessage(msg, ref selectedCategories);

            List<string> resultingFiles = getFilesForCategories(selectedCategories);

            ServiceMessage replyMsg = zipFilesRetrieverServerMessage(msg, resultingFiles);
            AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            dispatcher.PostMessage(replyMsg);
        }

        // disassebles the message sent by the client for this communicator
        void unzipFilesRetrieverClientMessage(ServiceMessage msg, ref List<string> selectedCategories)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());

            var categories = (from item in xDoc.Descendants() where item.Name == "Category" select item).ToList();
            foreach (var item in categories)
                selectedCategories.Add(item.Value);
        }

        // creates the server reply message for this communicator
        ServiceMessage zipFilesRetrieverServerMessage(ServiceMessage msg, List<string> resultingFiles)
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("FilesRetriever");
                xmlWriter.WriteStartElement("FileNames");
                foreach (var file in resultingFiles)
                    xmlWriter.WriteElementString("FileName", file);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, xmlDoc.OuterXml);
            replyMsg.TargetUrl = msg.SourceUrl;
            replyMsg.SourceUrl = msg.TargetUrl;
            return replyMsg;
        }

        // gets all files that belong to each of the given categories
        List<string> getFilesForCategories(List<string> givenCategories)
        {
            return FilesForCategories.getFilesForCategories(givenCategories, false);
        }
    }

    /// <summary>
    /// Retrieves text data content and metadata properties for a specified file
    /// </summary>
    class ServerContentRetrieverCommunicator : AbstractCommunicator
    {
        // constructor, initializes file sender
        public ServerContentRetrieverCommunicator()
        {
            serverFileSender.PrepareToSendFile("http://localhost:8090/ClientFileService");
        }

        FileSender serverFileSender = new FileSender();

        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            string fileName = msg.Contents.ToString();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> twoFiles = new List<string>();
            twoFiles.Add(Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\Vault", fileName).Single());
            twoFiles.Add(Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\Vault", Path.GetFileNameWithoutExtension(fileName) + ".xml").Single());
            foreach (var item in twoFiles)
                serverFileSender.SendFile(item);

            ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, fileName);
            replyMsg.TargetUrl = msg.SourceUrl;
            replyMsg.SourceUrl = msg.TargetUrl;
            AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            dispatcher.PostMessage(replyMsg);
        }
    }

    /// <summary>
    /// Sends the specified file to the client
    /// </summary>
    class ServerFileExtractorCommunicator : AbstractCommunicator
    {
        // constructor, initializes file sender
        public ServerFileExtractorCommunicator()
        {
            serverFileSender.PrepareToSendFile("http://localhost:8090/ClientFileService");
        }

        FileSender serverFileSender = new FileSender();

        // starts the processing of incoming messages
        protected override void ProcessMessages()
        {
            while (true)
            {
                ServiceMessage msg = bq.deQ();
                Console.Write("\n  {0} Received Message:\n", msg.TargetCommunicator);
                msg.ShowMessage();
                if ((string)msg.Contents == "quit")
                    break;

                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        void processServiceMsg(ServiceMessage msg)
        {
            string fileName = msg.Contents.ToString();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string requiredFile = Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\Vault", fileName).Single();
            serverFileSender.SendFile(requiredFile);

            //ServiceMessage replyMsg = ServiceMessage.MakeMessage(msg.SourceCommunicator, msg.TargetCommunicator, fileName);
            //replyMsg.TargetUrl = msg.SourceUrl;
            //replyMsg.SourceUrl = msg.TargetUrl;
            //AbstractMessageDispatcher dispatcher = AbstractMessageDispatcher.GetInstance();
            //dispatcher.PostMessage(replyMsg);
        }
    }
}
