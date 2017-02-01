/////////////////////////////////////////////////////////////////////
//	Provides functionality to search for text strings
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

/*
Public Interface

void PrintSearchResults - Prints the names of files that contain the search strings
void FindText - Finds search string in the content of text file
void DetermineTextStringsToSearch - Determines search string given at console
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;

namespace CTA
{
    public class TextFinder
    {
        public TextFinder(TypeOfSearch typeOfSearch)
        {
            //_textStringsToSearch = textStringsToSearch;
            _typeOfSearch = typeOfSearch;
        }

        List<string> _fileNames = new List<string>();
        List<string> _textStringsToSearch = new List<string>();
        TypeOfSearch _typeOfSearch;
        bool _allTextStringsNotFound;

        public List<string> FileNames { get { return _fileNames; } set { _fileNames = value; } }
        public TypeOfSearch TypeOfSearch { get { return _typeOfSearch; } set { _typeOfSearch = value; } }

        /// <summary>
        /// Prints the names of files that contain the search strings
        /// </summary>
        /// <param name="recursionActive">True if search for files goes recursively into child folders</param>
        public void PrintSearchResults(string recursionActive)
        {
            CommonMethods.PrintCharacters('=', 75, true, true);

            if (this.TypeOfSearch == TypeOfSearch.All)
                Console.WriteLine("Files containing all search strings with " + recursionActive + " -");
            if (this.TypeOfSearch == TypeOfSearch.AtleastOne)
                Console.WriteLine("Files containing at least one of the search strings with " + recursionActive + " -");
            CommonMethods.PrintCharacters('_', 75, false, true);

            if (this.FileNames.Count == 0)
            {
                Console.WriteLine("\n   NO RESULTS TO DISPLAY!\n\n");
                return;
            }

            foreach (var fileName in this.FileNames)
            {
                //Console.WriteLine(fileName.Substring(fileName.LastIndexOf("\\"), fileName.Length - fileName.LastIndexOf("\\")));
                Console.WriteLine(fileName + "\n");
            }

            CommonMethods.PrintCharacters('=', 75, true, true);
        }

        /// <summary>
        /// Finds search string in the content of text file
        /// </summary>
        /// <param name="fileName">File in which to look for search string</param>
        public void FindText(string fileName)
        {
            try
            {
                FileInfo givenFile = new FileInfo(fileName);
                if (givenFile.Extension == ".xml")
                    return;

                string lineOfTextFromFile = getLineOfText(fileName);

                if (_typeOfSearch == TypeOfSearch.All)
                {
                    handleTypeOfSearchAll(lineOfTextFromFile, fileName);
                    return;
                }

                if (_typeOfSearch == TypeOfSearch.AtleastOne)
                {
                    handleTypeOfSearchAtleastOne(lineOfTextFromFile, fileName);
                    return;
                }
            }
            catch (Exception)
            {
            }
        }

        // reads text from a file
        string getLineOfText(string fileName)
        {
            string lineOfTextFromFile = string.Empty;

            FileInfo givenFile = new FileInfo(fileName);
            if (givenFile.Extension == ".doc" || givenFile.Extension == ".docx")
            {
                _Application wordApp = new Application();
                _Document givenWordDoc = wordApp.Documents.Open(fileName);
                lineOfTextFromFile = givenWordDoc.Content.Text;

                givenWordDoc.Close(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
                wordApp.Quit(SaveChanges: WdSaveOptions.wdDoNotSaveChanges);
            }
            else
            {
                StreamReader rdr = File.OpenText(fileName);
                lineOfTextFromFile = rdr.ReadToEnd();
                rdr.Close();
            }

            return lineOfTextFromFile;
        }

        // handles the case where all search strings have to be matched
        void handleTypeOfSearchAll(string lineOfTextFromFile, string fileName)
        {
            _allTextStringsNotFound = false;

            foreach (var textString in _textStringsToSearch)
            {
                if (lineOfTextFromFile.IndexOf(textString) >= 0)
                    continue;
                else
                {
                    _allTextStringsNotFound = true;
                    break;
                }
            }

            if (!_allTextStringsNotFound)
                this.FileNames.Add(fileName);
        }

        // handles the case where atleast one search string has to be matched
        void handleTypeOfSearchAtleastOne(string lineOfTextFromFile, string fileName)
        {
            foreach (var textString in _textStringsToSearch)
            {
                if (lineOfTextFromFile.IndexOf(textString) >= 0)
                {
                    this.FileNames.Add(fileName);
                    break;
                }
            }
        }

        /// <summary>
        /// Determines search string given at console
        /// </summary>
        /// <param name="args">Contains search strings given at the console</param>
        public void DetermineTextStringsToSearch(List<string> args)
        {
            //for (int i = 0; i < args.Length; i++)
            //{
            //    if (args[i] == "/T")
            //        if (i + 1 == args.Length)
            //            Console.WriteLine("Command line switch /T is missing parameter!\n\n");
            //        else
            //            _textStringsToSearch.Add(args[i + 1].ToLower());
            //}

            _textStringsToSearch = args;
        }
    }
}

//TextFinderString1
//TextFinderString2
