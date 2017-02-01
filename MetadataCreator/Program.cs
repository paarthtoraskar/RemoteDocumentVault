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

namespace RDV
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 7)
                {
                    if (args.Length == 1 && (args[0] == "/?" || args[0] == "-?"))
                    {
                        Console.Write("\n   usage: MetadataCreator filePath /T textDescription /D <Dependency1> <Dependancy2> ... /K <Keyword1> <Keyword2> ... \n\n");
                        return;
                    }
                    Console.Write("\n   Too few arguments!, ");
                    return;
                }

                if (!File.Exists(args[0]))
                {
                    Console.Write("\n  File {0} does not exist!\n\n", args[0]);
                    return;
                }

                MetadataCreator mc = new MetadataCreator();
                //string xmlFileName = mc.DetermineXmlFileName(@"C:\Users\Paarth Toraskar\Documents\visual studio 2012\Projects\CTA\CTA\Navigate.cs");
                mc.DetermineMetadataProperties(args);
                mc.CreateMetadata();
            }
            catch (Exception)
            {
                Console.WriteLine("\n   ERROR! Unexpected shutdown!");
            }

        }
    }
}
