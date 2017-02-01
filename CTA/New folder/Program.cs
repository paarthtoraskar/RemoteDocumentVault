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
            if (args.Length < 2)
            {
                if (args.Length == 1 && (args[0] == "/?" || args[0] == "-?"))
                {
                    Console.Write("\n   usage: CTA folderPath filePattern /A|/O /T <textStringToSearch1> /T <textStringToSearch2> ... \n\n");
                    Console.Write("\n   usage: CTA folderPath filePattern /M <metadataProperty1> ,<metadataProperty2> ... \n\n");
                    return;
                }
                Console.Write("\n   Too few arguments!, ");
                return;
            }

            string path = args[0];
            string filePattern = args[1];

            if (!Directory.Exists(path))
            {
                Console.Write("\n  Path {0} does not exist!\n\n", path);
                return;
            }

            if (args.Contains("/T"))
            {
                if (!(args.Contains("/A") || args.Contains("/O")))
                {
                    Console.WriteLine("\n   Either of /A and /O need to be specified!\n\n");
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

                foreach (var fileName in textFinder.FileNames)
                {
                    Console.WriteLine(fileName.Substring(fileName.LastIndexOf("\\"), fileName.Length - fileName.LastIndexOf("\\")));
                    //Console.WriteLine(fileName);
                }
            }

            if (args.Contains("/M"))
            {
                MetadataRetriever metadataRetr = new MetadataRetriever();
                metadataRetr.DetermineMetadataPropsToRetrieve(args);

                Navigate nav = new Navigate(false);
                nav.newFile += new Navigate.newFileHandler(metadataRetr.RetrieveMetadata);
                nav.Go(path, filePattern);
            }
        }
    }
}

//TextFinderString1
