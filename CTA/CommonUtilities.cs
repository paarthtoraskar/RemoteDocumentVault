/////////////////////////////////////////////////////////////////////
//	Provides common utilities
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
Public Interface

void PrintCharacters - Prints a horizontal rule of characters
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA
{
    ///// <summary>
    ///// Stores the metadata property objects
    ///// </summary>
    //public struct MetadataProperty
    //{
    //    //public string FullName;
    //    public string Name;
    //    public DateTime LastModified;
    //    public string Version;
    //    public string Description;
    //    public List<string> Dependencies;
    //    public List<string> Keywords;
    //    public List<string> Categories;
    //}

    /// <summary>
    /// Stores the metadata property objects
    /// </summary>
    public class MetadataProperty
    {
        string name;
        DateTime lastModified;
        string version;
        string description;
        List<string> dependencies;
        List<string> keywords;
        List<string> categories;

        public string Name { get { return name; } set { name = value; } }
        public DateTime LastModified { get { return lastModified; } set { lastModified = value; } }
        public string Version { get { return version; } set { version = value; } }
        public string Description { get { return description; } set { description = value; } }
        public List<string> Dependencies { get { return dependencies; } set { dependencies = value; } }
        public List<string> Keywords { get { return keywords; } set { keywords = value; } }
        public List<string> Categories { get { return categories; } set { categories = value; } }

        public override string ToString()
        {
            StringBuilder metaProps = new StringBuilder();
            metaProps.Append("NAME - \t" + Name);
            metaProps.Append("\nLAST MODIFIED - \t" + LastModified.ToString());
            metaProps.Append("\nVERSION - \t" + Version);
            metaProps.Append("\nDESCRIPTION - \t" + Description);
            metaProps.Append("\nDEPENDENCIES - \n");
            foreach (var dependency in Dependencies)
                metaProps.Append(dependency + "\t");
            metaProps.Append("\nKEYWORDS - \n");
            foreach (var keyword in Keywords)
                metaProps.Append(keyword + "\t");
            metaProps.Append("\nCATEGORIES - \n");
            foreach (var category in Categories)
                metaProps.Append(category + "\t");
            return metaProps.ToString();
        }
    }

    /// <summary>
    /// Metadata properties
    /// </summary>
    public enum MetadataPropertyEnum
    {
        fullName,
        name,
        lastModified,
        version,
        description,
        dependencies,
        keywords,
        categories
    }

    /// <summary>
    /// Types of search
    /// </summary>
    public enum TypeOfSearch
    {
        AtleastOne, All
    }

    /// <summary>
    /// Common static methods
    /// </summary>
    public static class CommonMethods
    {
        /// <summary>
        /// Prints a horizontal rule of characters
        /// </summary>
        /// <param name="ch">Character to print</param>
        /// <param name="charLength">Number of times to print</param>
        /// <param name="precededByNewLine">True if preceded by new line</param>
        /// <param name="followedByNewLine">True if followed by new line</param>
        static public void PrintCharacters(char ch, int charLength, bool precededByNewLine, bool followedByNewLine)
        {
            if (precededByNewLine)
                Console.WriteLine("\n");
            string lineToPrint = string.Empty;
            for (int i = 0; i < charLength; i++)
                lineToPrint += ch;
            Console.WriteLine(lineToPrint);
            if (followedByNewLine)
                Console.WriteLine("\n");
        }
    }
}
