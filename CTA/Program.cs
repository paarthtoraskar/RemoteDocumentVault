/////////////////////////////////////////////////////////////////////
//	Start point for this executable
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Navig;

namespace CTA
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    if (args.Length == 1 && (args[0] == "/?" || args[0] == "-?"))
                    {
                        Console.Write("\n   usage: CTA folderPath filePattern {/R} /A|/O /T <textStringToSearch1> /T <textStringToSearch2> ... \n\n");
                        Console.Write("\n   usage: CTA folderPath filePattern {/R} /M <metadataProperty1>, <metadataProperty2> ... \n\n");
                        return;
                    }
                    Console.Write("\n   ERROR! Too few arguments!, ");
                    return;
                }

                string path = args[0];
                string filePattern = args[1];

                if (!Directory.Exists(path))
                {
                    Console.Write("\n  ERROR! Path {0} does not exist!\n\n", path);
                    return;
                }

                string recursionActive = "recursion not active";
                if (args.Contains("/R"))
                    recursionActive = "recursion active";

                if (args.Contains("/T"))
                    RunTextFinder(path, filePattern, args, recursionActive);

                if (args.Contains("/M"))
                    RunMetadataRetriever(path, filePattern, args, recursionActive);
            }
            catch (Exception)
            {
                Console.WriteLine("\n   ERROR! Unexpected shutdown!");
            }
        }

        /// <summary>
        /// Starts the text finder
        /// </summary>
        static void RunTextFinder(string path, string filePattern, string[] args, string recursionActive)
        {
            if (!(args.Contains("/A") || args.Contains("/O")))
            {
                Console.WriteLine("\n   ERROR! Either of /A and /O need to be specified!\n\n");
                return;
            }

            TextFinder textFinder = null;
            if (args.Contains("/A"))
                textFinder = new TextFinder(TypeOfSearch.All);
            if (args.Contains("/O"))
                textFinder = new TextFinder(TypeOfSearch.AtleastOne);
            textFinder.DetermineTextStringsToSearch(args);

            Navigate nav = new Navigate(args.Contains("/R"));
            nav.newFile += new Navigate.newFileHandler(textFinder.FindText);
            nav.Go(path, filePattern);

            textFinder.PrintSearchResults(recursionActive);
        }

        /// <summary>
        /// Starts the metadata properties retriever
        /// </summary>
        static void RunMetadataRetriever(string path, string filePattern, string[] args, string recursionActive)
        {
            MetadataRetriever metadataRetr = new MetadataRetriever();
            metadataRetr.DetermineMetadataPropsToRetrieve(args);

            if (metadataRetr.PropsToRetrieve.Count == 0)
            {
                Console.WriteLine("\n   ERROR! No metadata properties to retrieve!\n\n");
                return;
            }

            Navigate nav = new Navigate(args.Contains("/R"));
            nav.newFile += new Navigate.newFileHandler(metadataRetr.RetrieveMetadata);
            nav.Go(path, filePattern);

            metadataRetr.PrintRetrievedMetadata(recursionActive);
            metadataRetr.PrintFilesWithoutMetadata();
        }
    }
}

//TextFinderString1
