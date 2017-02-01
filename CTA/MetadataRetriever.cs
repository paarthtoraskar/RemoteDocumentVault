/////////////////////////////////////////////////////////////////////
//	Provides functionality to retrieve metadata
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
Public Interface

void PrintCharacters - Prints a horizontal rule of characters
void PrintFilesWithoutMetadata - Prints the retrieved metadata properties for files
void PrintRetrievedMetadata - Prints the retrieved metadata properties for files
void RetrieveMetadata - Retrieves metadata properties
void DetermineMetadataPropsToRetrieve - Determines metadata properties to retrieves
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CTA
{
    public class MetadataRetriever
    {
        List<string> _propsToRetrieve = new List<string>();
        List<KeyValuePair<string, MetadataProperty>> _retrievedMetadata = new List<KeyValuePair<string, MetadataProperty>>();
        List<string> _filesWithoutMetadata = new List<string>();

        public List<string> PropsToRetrieve { get { return _propsToRetrieve; } set { _propsToRetrieve = value; } }
        public List<KeyValuePair<string, MetadataProperty>> RetrievedMetadata { get { return _retrievedMetadata; } set { _retrievedMetadata = value; } }
        public List<string> FilesWithoutMetadata { get { return _filesWithoutMetadata; } set { _filesWithoutMetadata = value; } }

        /// <summary>
        /// Prints the list of files without a corresponding metadata file
        /// </summary>
        public void PrintFilesWithoutMetadata()
        {
            Console.WriteLine("Files missing metadata - ");
            CommonMethods.PrintCharacters('_', 75, false, true);

            if (this.FilesWithoutMetadata.Count == 0)
            {
                Console.WriteLine("\n   NO RESULTS TO DISPLAY!\n\n");
                return;
            }

            int count = 0;
            foreach (var fileName in this.FilesWithoutMetadata)
            {
                count++;
                Console.WriteLine("**" + count + " - FILE NAME - \n");
                Console.WriteLine(fileName + "\n");
            }
            CommonMethods.PrintCharacters('=', 75, false, true);
        }

        /// <summary>
        /// Prints the retrieved metadata properties for files
        /// </summary>
        /// <param name="recursionActive">True if search for files goes recursively into child folders</param>
        public void PrintRetrievedMetadata(string recursionActive)
        {
            CommonMethods.PrintCharacters('=', 75, true, true);
            Console.WriteLine("Requested metadata properties for the respective files with " + recursionActive + " -");
            CommonMethods.PrintCharacters('_', 75, false, true);

            if (this.RetrievedMetadata.Count == 0)
            {
                Console.WriteLine("\n   NO RESULTS TO DISPLAY!\n\n");
                return;
            }

            PrintRetrievedMetadataCore();
        }

        /// <summary>
        /// Prints the retrieved metadata properties for files
        /// </summary>
        /// <param name="recursionActive">True if search for files goes recursively into child folders</param>
        public void PrintRetrievedMetadataCore()
        {
            int count = 0;
            foreach (var fileName in this.RetrievedMetadata)
            {
                count++;
                Console.WriteLine("**" + count + " - FILE NAME - \n");
                Console.WriteLine(fileName.Key + "\n");
                CommonMethods.PrintCharacters('_', 25, false, true);

                Console.WriteLine("**" + count + " - METADATA PROPERTIES - \n");

                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.name.ToString()))
                    Console.WriteLine("\nNAME - " + fileName.Value.Name);

                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.lastModified.ToString()))
                    Console.WriteLine("\nLAST MODIFIED - " + fileName.Value.LastModified.ToString());

                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.version.ToString()))
                    Console.WriteLine("\nVERSION - " + fileName.Value.Version);


                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.description.ToString()))
                    Console.WriteLine("\nDESCRIPTION - " + fileName.Value.Description);

                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.dependencies.ToString()))
                {
                    Console.WriteLine("\nDEPENDENCIES - ");
                    foreach (var dependency in fileName.Value.Dependencies)
                        Console.WriteLine(dependency);
                }

                if (this.PropsToRetrieve.Contains(MetadataPropertyEnum.keywords.ToString()))
                {
                    Console.WriteLine("\nKEYWORDS - ");
                    foreach (var keyword in fileName.Value.Keywords)
                        Console.WriteLine(keyword);
                }
                CommonMethods.PrintCharacters('_', 50, false, true);
            }
            //CommonMethods.PrintCharacters('=', 75, false, true);
        }

        /// <summary>
        /// Retrieves metadata properties
        /// </summary>
        /// <param name="fileName">File for which to retrieve metadata</param>
        public void RetrieveMetadata(string fileName)
        {
            try
            {
                FileInfo givenFile = new FileInfo(fileName);
                if (givenFile.Extension == ".xml")
                    return;

                string xmlFileName = DetermineXmlFileName(fileName);
                if (!File.Exists(xmlFileName))
                {
                    _filesWithoutMetadata.Add(fileName);
                    return;
                }

                RetrieveMetadataCore(fileName, xmlFileName);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Retrieves metadata properties
        /// </summary>
        /// <param name="fileName">File for which to retrieve metadata</param>
        public void RetrieveMetadataCore(string fileName, string xmlFileName)
        {
            MetadataProperty props = new MetadataProperty();

            var xmlDoc = XDocument.Load(xmlFileName);
            if (_propsToRetrieve.Contains("Name"))
                props.Name = xmlDoc.Descendants("Name").Single().Value;
            if (_propsToRetrieve.Contains("LastModified"))
                props.LastModified = DateTime.Parse(xmlDoc.Descendants("LastModified").Single().Value);
            props.Version = string.Empty;
            if (_propsToRetrieve.Contains("Version"))
                props.Version = xmlDoc.Descendants("Version").Single().Value;
            if (_propsToRetrieve.Contains("Description"))
                props.Description = xmlDoc.Descendants("Description").Single().Value;
            props.Dependencies = new List<string>();
            if (_propsToRetrieve.Contains("Dependencies"))
            {
                var dependancies = xmlDoc.Descendants("Dependancy").ToList();
                foreach (var dependancy in dependancies)
                    props.Dependencies.Add(dependancy.Value.ToString());
            }
            props.Keywords = new List<string>();
            if (_propsToRetrieve.Contains("Keywords"))
            {
                var keywords = xmlDoc.Descendants("Keyword").ToList();
                foreach (var keyword in keywords)
                    props.Keywords.Add(keyword.Value.ToString());
            }
            _retrievedMetadata.Add(new KeyValuePair<string, MetadataProperty>(fileName, props));
        }

        /// <summary>
        /// Determines metadata properties to retrieves
        /// </summary>
        /// <param name="args">Contains names of properties passed at console</param>
        public void DetermineMetadataPropsToRetrieve(List<string> args)
        {
            //for (int i = args.ToList().IndexOf("/M") + 1; i < args.Length; i++)
            //{
            //    _propsToRetrieve.Add(args[i]);
            //}

            _propsToRetrieve = args;
        }

        /// <summary>
        /// Determines the name of the XML file for the corresponding text file
        /// </summary>
        /// <param name="fileName">Name of text file</param>
        /// <returns>Name of XML file</returns>
        string DetermineXmlFileName(string fileName)
        {
            return fileName.Replace(fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.')), ".xml");
        }
    }
}