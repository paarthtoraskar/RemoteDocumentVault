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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;

namespace RDV
{
    /// <summary>
    /// Interaction logic for MetadataRetriever.xaml
    /// </summary>
    public partial class MetadataRetriever : Page
    {
        ClientMetadataRetrieverCommunicator clientMetadataRetrieverCommunicator;
        ClientCategoriesRetrieverCommunicator clientCategoriesRetrieverCommunicatorInMR;
        ClientContentRetrieverCommunicator clientContentRetrieverCommunicatorInMR;

        RetrievedMetadataViewModel retrievedMetadata = new RetrievedMetadataViewModel();

        public RetrievedMetadataViewModel RetrievedMetadata
        {
            get
            {
                return retrievedMetadata;
            }
            set
            {
                if (retrievedMetadata != value)
                    retrievedMetadata = value;
            }
        }

        // constructor initializes the various communicators
        public MetadataRetriever()
        {
            InitializeComponent();

            clientMetadataRetrieverCommunicator = new ClientMetadataRetrieverCommunicator();
            clientMetadataRetrieverCommunicator.Name = "ClientMetadataRetrieverCommunicator";
            clientMetadataRetrieverCommunicator.UpdateUI = new Action<List<KeyValuePair<string, MetadataProperty>>>(addMetadataToListBox);
            App.clientReceiver.Register(clientMetadataRetrieverCommunicator);
            clientMetadataRetrieverCommunicator.Start();

            clientCategoriesRetrieverCommunicatorInMR = new ClientCategoriesRetrieverCommunicator();
            clientCategoriesRetrieverCommunicatorInMR.Name = "ClientCategoriesRetrieverCommunicatorInMR";
            clientCategoriesRetrieverCommunicatorInMR.UpdateData = new Action<List<string>>(addCategoriesToList);
            App.clientReceiver.Register(clientCategoriesRetrieverCommunicatorInMR);
            clientCategoriesRetrieverCommunicatorInMR.Start();

            clientContentRetrieverCommunicatorInMR = new ClientContentRetrieverCommunicator();
            clientContentRetrieverCommunicatorInMR.Name = "ClientContentRetrieverCommunicatorInMR";
            clientContentRetrieverCommunicatorInMR.UpdateUI = new Action<string, MetadataProperty>(addContentToTab);
            App.clientReceiver.Register(clientContentRetrieverCommunicatorInMR);
            clientContentRetrieverCommunicatorInMR.Start();

            this.DataContext = RetrievedMetadata;
        }

        // gets categories that span across all files in the vault
        void getCategories()
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerCategoriesRetrieverCommunicator", "ClientCategoriesRetrieverCommunicatorInMR", null);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            App.clientSender.PostMessage(msg);
        }

        private void buttonRetrieve_Click(object sender, RoutedEventArgs e)
        {
            if (noPropSelected())
            {
                MessageBox.Show("Select atleast one property!");
                return;
            }
            ServiceMessage msg = zipMetadataRetrieverClientMessage();
            App.clientSender.PostMessage(msg);
        }

        // checks if no metadata properties have been selected
        bool noPropSelected()
        {
            if (!(bool)checkBoxName.IsChecked && !(bool)checkBoxLastMod.IsChecked && !(bool)checkBoxVersion.IsChecked &&
                !(bool)checkBoxDesc.IsChecked && !(bool)checkBoxDep.IsChecked && !(bool)checkBoxKeywords.IsChecked)
                return true;
            return false;
        }

        // creates the client query message for this communicator
        ServiceMessage zipMetadataRetrieverClientMessage()
        {
            List<string> metaProps = getMetaProps();

            XmlDocument xmlDoc = new XmlDocument();
            using (XmlWriter xmlWriter = xmlDoc.CreateNavigator().AppendChild())
            {
                xmlWriter.WriteStartElement("MetadataRetriever");
                xmlWriter.WriteStartElement("MetaProps");
                foreach (var item in metaProps)
                    xmlWriter.WriteElementString("MetaProp", item);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("SelectedCategories");
                foreach (var item in listBoxCategories.SelectedItems)
                    xmlWriter.WriteElementString("Category", item.ToString());
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            ServiceMessage msg = ServiceMessage.MakeMessage("ServerMetadataRetrieverCommunicator", "ClientMetadataRetrieverCommunicator", xmlDoc.OuterXml);
            //ServiceMessage msg = ServiceMessage.MakeMessage("ServerMetadataRetrieverCommunicator", "ClientMetadataRetrieverCommunicator", string.Empty);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;

            return msg;
        }

        // gets the metadata props that have been selected to be queried
        List<string> getMetaProps()
        {
            List<string> metaProps = new List<string>();
            if ((bool)checkBoxName.IsChecked)
                metaProps.Add(checkBoxName.Tag.ToString());
            if ((bool)checkBoxLastMod.IsChecked)
                metaProps.Add(checkBoxLastMod.Tag.ToString());
            if ((bool)checkBoxVersion.IsChecked)
                metaProps.Add(checkBoxVersion.Tag.ToString());
            if ((bool)checkBoxDesc.IsChecked)
                metaProps.Add(checkBoxDesc.Tag.ToString());
            if ((bool)checkBoxDep.IsChecked)
                metaProps.Add(checkBoxDep.Tag.ToString());
            if ((bool)checkBoxKeywords.IsChecked)
                metaProps.Add(checkBoxKeywords.Tag.ToString());

            return metaProps;
        }

        // adds retrieved metadata properties to text block
        void addMetadataToTextBlock(List<KeyValuePair<string, MetadataProperty>> retrievedMetadata)
        {
            textBlockRetrievedMetadata.Text = string.Empty;
            if (retrievedMetadata.Count == 0)
            {
                textBlockRetrievedMetadata.Text += ("\n   NO RESULTS TO DISPLAY!\n\n");
                return;
            }
            PrintRetrievedMetadata(retrievedMetadata);
        }

        // adds retrieved metadata properties to list box
        void addMetadataToListBox(List<KeyValuePair<string, MetadataProperty>> retrievedMetadata)
        {
            RetrievedMetadata.RetrievedMetadataList = retrievedMetadata;
        }

        // formats the retrieved metadata properties being added to text block
        public void PrintRetrievedMetadata(List<KeyValuePair<string, MetadataProperty>> retrievedMetadata)
        {
            int count = 0;
            foreach (var fileName in retrievedMetadata)
            {
                count++;
                textBlockRetrievedMetadata.Text += ("\n\n**" + count + " - FILE NAME - \n");
                textBlockRetrievedMetadata.Text += (fileName.Key + "\n");

                textBlockRetrievedMetadata.Text += ("\n**" + count + " - METADATA PROPERTIES -");

                if ((bool)checkBoxName.IsChecked)
                    textBlockRetrievedMetadata.Text += ("\nNAME - " + fileName.Value.Name);
                if ((bool)checkBoxLastMod.IsChecked)
                    textBlockRetrievedMetadata.Text += ("\nLAST MODIFIED - " + fileName.Value.LastModified.ToString());
                if ((bool)checkBoxVersion.IsChecked)
                    textBlockRetrievedMetadata.Text += ("\nVERSION - " + fileName.Value.Version);
                if ((bool)checkBoxDesc.IsChecked)
                    textBlockRetrievedMetadata.Text += ("\nDESCRIPTION - " + fileName.Value.Description);
                if ((bool)checkBoxDep.IsChecked)
                {
                    textBlockRetrievedMetadata.Text += ("\nDEPENDENCIES - \n");
                    foreach (var dependency in fileName.Value.Dependencies)
                        textBlockRetrievedMetadata.Text += (dependency + "\n");
                }
                if ((bool)checkBoxKeywords.IsChecked)
                {
                    textBlockRetrievedMetadata.Text += ("\nKEYWORDS - \n");
                    foreach (var keyword in fileName.Value.Keywords)
                        textBlockRetrievedMetadata.Text += (keyword + "\n");
                }
                //textBlockRetrievedMetadata.Text += ("\n\n");
            }
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

        private void listBoxProps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListBox).SelectedValue == null)
                return;
            ServiceMessage msg = zipContentRetrieverClientMessage(Path.GetFileName(((KeyValuePair<string, MetadataProperty>)((sender as ListBox).SelectedValue)).Key));
            App.clientSender.PostMessage(msg);
        }

        // creates the client query message for this communicator
        ServiceMessage zipContentRetrieverClientMessage(string selectedFileName)
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerContentRetrieverCommunicator", "ClientContentRetrieverCommunicatorInMR", selectedFileName);
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
    /// Retrieves specified metadata properties for all files
    /// in the vault that belong to a specified categories
    /// </summary>
    class ClientMetadataRetrieverCommunicator : AbstractCommunicator
    {
        public Action<List<KeyValuePair<string, MetadataProperty>>> UpdateUI;

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
                        Console.Write("\n  ClientMetadataRetrieverCommunicator shutting down");

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
            List<KeyValuePair<string, MetadataProperty>> retrievedMetadata = new List<KeyValuePair<string, MetadataProperty>>();
            unzipMetadataRetrieverServerMessage(msg, ref retrievedMetadata);

            if (Application.Current.Dispatcher.CheckAccess())
                UpdateUI(retrievedMetadata);
            else
                Application.Current.Dispatcher.Invoke(
                  UpdateUI,
                  System.Windows.Threading.DispatcherPriority.Background,
                  new object[] { retrievedMetadata }
                );
        }

        // disassebles the message sent by the server for this communicator
        void unzipMetadataRetrieverServerMessage(ServiceMessage msg, ref List<KeyValuePair<string, MetadataProperty>> retrievedMetadata)
        {
            XDocument xDoc = XDocument.Parse(msg.Contents.ToString());
            var metadata = xDoc.Descendants("Metadatum");

            foreach (var item in metadata)
            {
                MetadataProperty currMeta = new MetadataProperty();
                string fullName = item.Descendants("FullName").Single().Value;
                currMeta.Name = item.Descendants("Name").Single().Value;
                currMeta.LastModified = DateTime.Parse(item.Descendants("LastModified").Single().Value);
                currMeta.Version = item.Descendants("Version").Single().Value;
                currMeta.Description = item.Descendants("Description").Single().Value;
                var dependancies = item.Descendants("Dependancy").ToList();
                currMeta.Dependencies = new List<string>();
                foreach (var dependancy in dependancies)
                    currMeta.Dependencies.Add(dependancy.Value.ToString());
                var keywords = item.Descendants("Keyword").ToList();
                currMeta.Keywords = new List<string>();
                foreach (var keyword in keywords)
                    currMeta.Keywords.Add(keyword.Value.ToString());
                retrievedMetadata.Add(new KeyValuePair<string, MetadataProperty>(fullName, currMeta));
            }

        }
    }

    public class RetrievedMetadataViewModel : INotifyPropertyChanged
    {
        List<KeyValuePair<string, MetadataProperty>> retrievedMetadataList;

        public List<KeyValuePair<string, MetadataProperty>> RetrievedMetadataList
        {
            get
            {
                return retrievedMetadataList;
            }
            set
            {
                if (retrievedMetadataList != value)
                {
                    retrievedMetadataList = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // notifies GUI when the list of metadata properties changes so binding can be updated
        void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}