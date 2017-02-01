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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Interaction logic for Categories.xaml
    /// </summary>
    public partial class CategoriesView : Page
    {
        ClientCategoriesRetrieverCommunicator clientCategoriesRetrieverCommunicatorInCV;
        ClientFilesRetrieverCommunicator clientFilesRetrieverCommunicator;
        ClientContentRetrieverCommunicator clientContentRetrieverCommunicatorInCV;

        // constructor initializes the various communicators
        public CategoriesView()
        {
            InitializeComponent();

            clientCategoriesRetrieverCommunicatorInCV = new ClientCategoriesRetrieverCommunicator();
            clientCategoriesRetrieverCommunicatorInCV.Name = "ClientCategoriesRetrieverCommunicatorInCV";
            clientCategoriesRetrieverCommunicatorInCV.UpdateData = new Action<List<string>>(addCategoriesToList);
            App.clientReceiver.Register(clientCategoriesRetrieverCommunicatorInCV);
            clientCategoriesRetrieverCommunicatorInCV.Start();

            clientFilesRetrieverCommunicator = new ClientFilesRetrieverCommunicator();
            clientFilesRetrieverCommunicator.Name = "ClientFilesRetrieverCommunicator";
            clientFilesRetrieverCommunicator.UpdateUI = new Action<List<string>>(addFilesToList);
            App.clientReceiver.Register(clientFilesRetrieverCommunicator);
            clientFilesRetrieverCommunicator.Start();

            clientContentRetrieverCommunicatorInCV = new ClientContentRetrieverCommunicator();
            clientContentRetrieverCommunicatorInCV.Name = "ClientContentRetrieverCommunicatorInCV";
            clientContentRetrieverCommunicatorInCV.UpdateUI = new Action<string, MetadataProperty>(addContentToTab);
            App.clientReceiver.Register(clientContentRetrieverCommunicatorInCV);
            clientContentRetrieverCommunicatorInCV.Start();
        }

        // gets categories that span across all files in the vault
        void getCategories()
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerCategoriesRetrieverCommunicator", "ClientCategoriesRetrieverCommunicatorInCV", null);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            App.clientSender.PostMessage(msg);
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
            listBoxFiles.Items.Clear();
        }

        private void buttonGetFiles_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxCategories.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select categories!");
                return;
            }
            ServiceMessage msg = zipFilesRetrieverClientMessage();
            App.clientSender.PostMessage(msg);
        }

        // creates the client query message for this communicator
        ServiceMessage zipFilesRetrieverClientMessage()
        {
            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("FilesRetriever");
                xmlWriter.WriteStartElement("Categories");
                foreach (var item in listBoxCategories.SelectedItems)
                    xmlWriter.WriteElementString("Category", item.ToString());
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage msg = ServiceMessage.MakeMessage("ServerFilesRetrieverCommunicator", "ClientFilesRetrieverCommunicator", xmlDoc.OuterXml);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            return msg;
        }

        // adds the retrieved files for categories to the list box
        void addFilesToList(List<string> fileNames)
        {
            //if (fileNames.Count == 0)
            //{
            //    MessageBox.Show("No files belong to these categories!");
            //    return;
            //}
            listBoxFiles.Items.Clear();
            foreach (var item in fileNames)
                listBoxFiles.Items.Add(item);
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
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerContentRetrieverCommunicator", "ClientContentRetrieverCommunicatorInCV", selectedFileName);
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
    /// Retrieves files that belong to a specified categories
    /// </summary>
    class ClientFilesRetrieverCommunicator : AbstractCommunicator
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
                        Console.Write("\n  ClientFilesRetrieverCommunicator shutting down");

                    // shut down dispatcher

                    msg.TargetCommunicator = "dispatcher";
                    AbstractMessageDispatcher.GetInstance().PostMessage(msg);
                    break;
                }
                processServiceMsg(msg);
            }
        }

        // performs specific actions related to this communicator on the incoming message
        private void processServiceMsg(ServiceMessage msg)
        {
            List<string> fileNames = new List<string>();
            unzipFilesRetrieverServerMessage(msg, ref fileNames);

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
        void unzipFilesRetrieverServerMessage(ServiceMessage msg, ref List<string> fileNames)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());
            var ts = (from item in xDoc.Descendants() where item.Name == "FileName" select item).ToList();
            foreach (var item in ts)
                fileNames.Add(item.Value);
        }
    }
}
