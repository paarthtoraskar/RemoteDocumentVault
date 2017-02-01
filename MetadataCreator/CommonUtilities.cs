/////////////////////////////////////////////////////////////////////
//	Provides common utilities
//
//	Paarth Toraskar
//	SU - MSCE - S/W
//	pbtorask@syr.edu
/////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC
{
    /// <summary>
    /// Stores the metadata property objects
    /// </summary>
    struct MetadataProperty
    {
        public string FullName;
        public string Name;
        public DateTime LastModified;
        public string Version;
        public string Description;
        public List<string> Dependencies;
        public List<string> Keywords;
        public List<string> Categories;
    }

    /// <summary>
    /// Metadata properties
    /// </summary>
    enum MetadataPropertyEnum
    {
        name,
        lastModified,
        version,
        description,
        dependencies,
        keywords,
        categories
    }
}
