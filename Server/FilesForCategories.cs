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

namespace RDV
{
    abstract class FilesForCategories
    {
        // gets all files that belong to each of the categories provided
        public static List<string> getFilesForCategories(List<string> givenCategories, bool getfullPath)
        {
            List<string> filesForCategories = new List<string>();
            List<string> categoriesForThisFile = new List<string>();
            string loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            List<string> xmlfiles = Directory.GetFiles(Directory.GetParent(loc).Parent.FullName + @"\Vault", "*.xml").ToList();

            foreach (var item in xmlfiles)
            {
                var xmlDoc = XDocument.Load(item);
                var categoriesXElements = xmlDoc.Descendants("Category").ToList();
                categoriesForThisFile.Clear();
                foreach (var category in categoriesXElements)
                    categoriesForThisFile.Add(category.Value);
                if (fileBelongsToCategories(givenCategories, categoriesForThisFile))
                {
                    //if (getfullPath)
                    //    filesForCategories.Add(getActualFileNameFromXmlFileName(item));
                    //else
                    //    filesForCategories.Add(Path.GetFileNameWithoutExtension(item));

                    filesForCategories.Add(getActualFileNameFromXmlFileName(item));
                }
            }
            return filesForCategories;
        }

        // checks if file belongs to each of the given categories
        static bool fileBelongsToCategories(List<string> givenCategories, List<string> categoriesForThisFile)
        {
            bool fileBelongsToCategories = true;
            foreach (var givenCat in givenCategories)
            {
                if (!categoriesForThisFile.Contains(givenCat))
                {
                    fileBelongsToCategories = false;
                    return fileBelongsToCategories;
                }
            }
            return fileBelongsToCategories;
        }

        // get the name of the data file from its metadata file
        static string getActualFileNameFromXmlFileName(string xmlFileName)
        {
            //FileInfo xmlFileInfo = new FileInfo(xmlFileName);
            //List<string> twoFiles = Directory.GetFiles(xmlFileInfo.DirectoryName, Path.GetFileNameWithoutExtension(xmlFileName) + ".*").ToList();
            //twoFiles.Remove(xmlFileName);
            //if (twoFiles.Count == 1)
            //    return twoFiles[0];
            //else
            //    return "Something went wrong!";

            XDocument xDoc = XDocument.Load(xmlFileName);
            string actualFileName = xDoc.Descendants("Name").Single().Value;
            return actualFileName;
        }
    }
}
