/////////////////////////////////////////////////////////////////////
//	Provides functionality to create metadata files
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
Public Interface

void DetermineMetadataProperties - Determines the metadata properties and their values given at the console
void CreateMetadata - Writes the metadata to file
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MC
{
    public class MetadataCreator
    {
        MetadataProperty _metaProp;

        /// <summary>
        /// Determines the metadata properties and their values given at the console
        /// </summary>
        /// <param name="args">Contains names of properties passed at console</param>
        public void DetermineMetadataProperties(string file, string desc, List<string> dep, List<string> keywords, List<string> categories)
        {
            FileInfo fileInfo = new FileInfo(file);
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(file);

            _metaProp.FullName = fileInfo.FullName;
            _metaProp.Name = fileInfo.Name;
            _metaProp.LastModified = fileInfo.LastWriteTime;
            _metaProp.Version = fileVersionInfo.FileVersion;

            _metaProp.Description = desc;
            _metaProp.Dependencies = dep;
            _metaProp.Keywords = keywords;
            _metaProp.Categories = categories;
        }

        /// <summary>
        /// Writes the metadata to file
        /// </summary>
        /// <returns>Complete file name of the metadata file created</returns>
        public string CreateMetadata()
        {
            string xmlFileName = DetermineXmlFileName(_metaProp.Name);
            XmlWriter xmlWriter = XmlWriter.Create(xmlFileName);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("XmlMetadata");

            xmlWriter.WriteElementString("Name", _metaProp.Name);
            xmlWriter.WriteElementString("LastModified", _metaProp.LastModified.ToString());
            xmlWriter.WriteElementString("Version", _metaProp.Version);
            xmlWriter.WriteElementString("Description", _metaProp.Description);

            xmlWriter.WriteStartElement("Dependancies");
            foreach (var dependancy in _metaProp.Dependencies)
            {
                xmlWriter.WriteElementString("Dependancy", dependancy);
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Keywords");
            foreach (var keyword in _metaProp.Keywords)
            {
                xmlWriter.WriteElementString("Keyword", keyword);
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Categories");
            foreach (var category in _metaProp.Categories)
            {
                xmlWriter.WriteElementString("Category", category);
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();

            xmlWriter.Close();

            Console.WriteLine("\n   SUCCESS! Metadata file has been created!\n\n");

            return xmlFileName;
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
