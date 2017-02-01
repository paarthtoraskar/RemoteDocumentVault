using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTA
{
    class TextFinder
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

        public void FindText(string fileName)
        {
            try
            {
                StreamReader rdr = File.OpenText(fileName);
                string line = rdr.ReadToEnd();

                if (_typeOfSearch == TypeOfSearch.All)
                {
                    _allTextStringsNotFound = false;

                    foreach (var textString in _textStringsToSearch)
                    {
                        if (line.ToLower().IndexOf(textString) >= 0)
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

                if (_typeOfSearch == TypeOfSearch.AtleastOne)
                {
                    foreach (var textString in _textStringsToSearch)
                    {
                        if (line.ToLower().IndexOf(textString) >= 0)
                        {
                            this.FileNames.Add(fileName);
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        public void DetermineTextStringsToSearch(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "/T")
                    if (i + 1 == args.Length)
                        Console.WriteLine("Command line switch /T is missing parameter!\n\n");
                    else
                        _textStringsToSearch.Add(args[i + 1].ToLower());
            }
        }
    }
}

//TextFinderString1
//TextFinderString2
