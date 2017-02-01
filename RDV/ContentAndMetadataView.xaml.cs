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

namespace RDV
{
    /// <summary>
    /// Interaction logic for ContentAndMetadataView.xaml
    /// </summary>
    public partial class ContentAndMetadataView : Page
    {
        public ContentAndMetadataView()
        {
            InitializeComponent();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            clearFields();
        }

        // clears all data from fields on page
        void clearFields()
        {
            textBlockName.Text = string.Empty;
            textBlockContent.Text = string.Empty;
            textBlockMetadata.Text = string.Empty;
        }

        private void buttonExtract_Click(object sender, RoutedEventArgs e)
        {
            if (textBlockName.Text == string.Empty)
            {
                MessageBox.Show("No file loaded in view!");
                return;
            }
            ServiceMessage msg = zipFileExtractorClientMessage(textBlockName.Text);
            App.clientSender.PostMessage(msg);

            MessageBox.Show("The files has been extracted to - " + ClientFileReceiver.FilePath);
        }

        // creates the client query message for this communicator
        ServiceMessage zipFileExtractorClientMessage(string selectedFileName)
        {
            ServiceMessage msg = ServiceMessage.MakeMessage("ServerFileExtractorCommunicator", null, selectedFileName);
            msg.SourceUrl = App.ClientUrl;
            msg.TargetUrl = App.ServerUrl;
            return msg;
        }
    }
}
