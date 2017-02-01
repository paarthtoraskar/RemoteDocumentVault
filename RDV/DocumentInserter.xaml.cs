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

using Microsoft.Win32;
using System.IO;
using MC;

namespace RDV
{
    /// <summary>
    /// Interaction logic for DocumentInserter.xaml
    /// </summary>
    public partial class DocumentInserter : Page
    {
        FileSender clientFileSender = new FileSender();

        // initializes the file sender
        public DocumentInserter()
        {
            InitializeComponent();
            clientFileSender.PrepareToSendFile("http://localhost:8080/ServerFileService");
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            if ((bool)fileDialog.ShowDialog())
            {
                //FileSender.SendFile(fileDialog.FileName);
                textBoxSelectedFile.Text = fileDialog.FileName;
            }
        }

        private void buttonInsert_Click(object sender, RoutedEventArgs e)
        {
            if (!FileSelected())
            {
                MessageBox.Show("Select a file!");
                return;
            }

            if (!AllMetaPropsEntered())
            {
                MessageBox.Show("Enter description and categories!");
                return;
            }

            if (File.Exists(textBoxSelectedFile.Text))
                if (clientFileSender.SendFile(textBoxSelectedFile.Text))
                {
                    MetadataCreator mc = new MetadataCreator();
                    mc.DetermineMetadataProperties(textBoxSelectedFile.Text, textBoxDesc.Text, getDep(), getKeywords(), getCategories());
                    string metaPropsFile = mc.CreateMetadata();
                    clientFileSender.SendFile(metaPropsFile);
                    File.Delete(metaPropsFile);
                    MessageBox.Show("File inserted successfully!");
                    ClearPage();
                }

        }

        // checks if a file is selected for insertion into the vault
        bool FileSelected()
        {
            if (textBoxSelectedFile.Text == string.Empty)
                return false;
            return true;
        }

        // checks if required metadata properties have been entered
        bool AllMetaPropsEntered()
        {
            if (textBoxDesc.Text.Trim() == string.Empty || textBoxCategories.Text.Trim() == string.Empty)
                return false;
            return true;
        }

        // makes a list of dependencies from those entered in the text box
        List<string> getDep()
        {
            List<string> dep = new List<string>();
            List<string> tempDep = textBoxDep.Text.Split(new char[] { ',', ';', '.', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tempDep)
            {
                string trimmedItem = item.Trim();
                if (!(trimmedItem == string.Empty) && !(dep.Contains(trimmedItem)))
                    dep.Add(trimmedItem);
            }
            return dep;
        }

        // makes a list of keywords from those entered in the text box
        List<string> getKeywords()
        {
            List<string> keywords = new List<string>();
            List<string> tempKeywords = textBoxKeywords.Text.Split(new char[] { ',', ';', '.', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tempKeywords)
            {
                string trimmedItem = item.Trim();
                if (!(trimmedItem == string.Empty) && !(keywords.Contains(trimmedItem)))
                    keywords.Add(trimmedItem);
            }
            return keywords;
        }

        // makes a list of categories from those entered in the text box
        List<string> getCategories()
        {
            List<string> categories = new List<string>();
            List<string> tempCategories = textBoxCategories.Text.Split(new char[] { ',', ';', '.', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var item in tempCategories)
            {
                string trimmedItem = item.Trim();
                if (!(trimmedItem == string.Empty) && !(categories.Contains(trimmedItem)))
                    categories.Add(trimmedItem);
            }
            return categories;
        }

        // clears all data from fields on page
        void ClearPage()
        {
            textBoxSelectedFile.Clear();
            textBoxDesc.Clear();
            textBoxDep.Clear();
            textBoxKeywords.Clear();
            textBoxCategories.Clear();
        }
    }
}
