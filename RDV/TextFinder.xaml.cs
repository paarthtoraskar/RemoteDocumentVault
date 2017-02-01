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

using CTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Interaction logic for TextFinder.xaml
    /// </summary>
    public partial class TextFinder : Page
    {
        ClientTextFinderCommunicator clientTextFinderCommunicator;
        ClientCategoriesRetrieverCommunicator clientCategoriesRetrieverCommunicatorInTF;
        ClientContentRetrieverCommunicator clientContentRetrieverCommunicatorInTF;

        // constructor initializes the various communicators
        public TextFinder()
        {
            InitializeComponent();

            clientTextFinderCommunicator = new ClientTextFinderCommunicator();
            clientTextFinderCommunicator.Name = "ClientTextFinderCommunicator";
            clientTextFinderCommunicator.UpdateUI = new Action<List<string>>(addFilesToList);
            App.clientReceiver.Register(clientTextFinderCommunicator);
            clientTextFinderCommunicator.Start();

            clientCategoriesRetrieverCommunicatorInTF = new ClientCategoriesRetrieverCommunicator();
            clientCategoriesRetrieverCommunicatorInTF.Name = "ClientCategoriesRetrieverCommunicatorInTF";
            clientCategoriesRetrieverCommunicatorInTF.UpdateData = new Action<List<string>>(addCategoriesToList);
            App.clientReceiver.Register(clientCategoriesRetrieverCommunicatorInTF);
            clientCategoriesRetrieverCommunicatorInTF.Start();

            clientContentRetrieverCommunicatorInTF = new ClientContentRetrieverCommunicator();
            clientContentRetrieverCommunicatorInTF.Name = "ClientContentRetrieverCommunicatorInTF";
            clientContentRetrieverCommunicatorInTF.UpdateUI = new Action<string, MetadataProperty>(addContentToTab);
            App.clientReceiver.Register(clientContentRetrieverCommunicatorInTF);
            clientContentRetrieverCommunicatorInTF.Start();
        }

        // gets categories that span across all files in the vault
        void getCategories()
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerCategoriesRetrieverCommunicator", "ClientCategoriesRetrieverCommunicatorInTF", null);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            App.clientSender.PostMessage(msg);
        }

        private void buttonFind_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxTextStrings.Text == string.Empty)
            {
                MessageBox.Show("Enter text strings to search!");
                return;
            }

            ServiceMessage msg = zipTextFinderClientMessage();
            App.clientSender.PostMessage(msg);
        }

        // creates the client query message for this communicator
        ServiceMessage zipTextFinderClientMessage()
        {
            List<string> textStrings = getTextStrings();

            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("TextFinder");
                xmlWriter.WriteElementString("AllOrOne", comboAllOrOne.Text);
                xmlWriter.WriteStartElement("TextStrings");
                foreach (var item in textStrings)
                    xmlWriter.WriteElementString("TextString", item);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("SelectedCategories");
                foreach (var item in listBoxCategories.SelectedItems)
                    xmlWriter.WriteElementString("Category", item.ToString());
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage msg = ServiceMessage.MakeMessage("ServerTextFinderCommunicator", "ClientTextFinderCommunicator", xmlDoc.OuterXml);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            return msg;
        }

        // gets the search text strings that have been selected to be queried
        List<string> getTextStrings()
        {
            List<string> textStrings = new List<string>();
            List<string> tempTextStrings = textBoxTextStrings.Text.Split(new char[] { ',', ';', '.', ':' }).ToList();
            foreach (var item in tempTextStrings)
            {
                string trimmedItem = item.Trim();
                if (!(trimmedItem == string.Empty))
                    textStrings.Add(trimmedItem);
            }
            return textStrings;
        }

        // adds retrieved files to list box
        void addFilesToList(List<string> fileNames)
        {
            listBoxFiles.Items.Clear();
            foreach (var item in fileNames)
                listBoxFiles.Items.Add(item);
        }

        // add the retrieved categories to the list box
        void addCategoriesToList(List<string> categories)
        {
            listBoxCategories.Items.Clear();
            foreach (var item in categories)
                listBoxCategories.Items.Add(item);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            getCategories();
        }

        private void listBoxFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedValue == null)
                return;
            ServiceMessage msg = zipContentRetrieverClientMessage((sender as ListBox).SelectedValue.ToString());
            App.clientSender.PostMessage(msg);
        }

        // creates the client query message for this communicator
        ServiceMessage zipContentRetrieverClientMessage(string selectedFileName)
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerContentRetrieverCommunicator", "ClientContentRetrieverCommunicatorInTF", selectedFileName);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            return msg;
        }

        // adds the retrieved content and metadata to the tab view
        void addContentToTab(string allTextOfActualFile, MetadataProperty readProps)
        {
            TabControl mainWindowTabControl = App.Current.MainWindow.FindName("tabControlMainFeatures") as TabControl;
            Frame frameContentAndMetadata = mainWindowTabControl.FindName("frameContentAndMetadata") as Frame;
            Page pageContentAndMetadataView = frameContentAndMetadata.Content as Page;

            (pageContentAndMetadataView.FindName("textBlockName") as TextBlock).Text = readProps.Name;
            (pageContentAndMetadataView.FindName("textBlockContent") as TextBlock).Text = allTextOfActualFile;
            (pageContentAndMetadataView.FindName("textBlockMetadata") as TextBlock).Text = readProps.ToString();
            mainWindowTabControl.SelectedIndex = 4;
        }
    }

    /// <summary>
    /// Finds text strings in files in the vault that belong to a specified categories
    /// and returns those file names
    /// </summary>
    class ClientTextFinderCommunicator : AbstractCommunicator
    {
        public Action<List<string>> UpdateUI;

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
                        Console.Write("\n  ClientTextFinderCommunicator shutting down");

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
            List<string> fileNames = new List<string>();
            unzipTextFinderServerMessage(msg, ref fileNames);

            if (Application.Current.Dispatcher.CheckAccess())
                UpdateUI(fileNames);
            else
                Application.Current.Dispatcher.Invoke(
                  UpdateUI,
                  System.Windows.Threading.DispatcherPriority.Background,
                  new object[] { fileNames }
                );
        }

        // disassebles the message sent by the server for this communicator
        void unzipTextFinderServerMessage(ServiceMessage msg, ref List<string> fileNames)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());
            var ts = (from item in xDoc.Descendants() where item.Name == "FileName" select item).ToList();
            foreach (var item in ts)
                fileNames.Add(item.Value);
        }
    }
}
