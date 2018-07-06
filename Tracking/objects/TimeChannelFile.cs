using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking.objects
{
    class TimeChannelFile
    {
        public TimeChannelFile()
        {

        }

         public string parentFolder { get; set; }
         public string fileName { get; set; }
         public string fullFileName { get; set; }

        public void setFileName()
        {
            fileName = Path.GetFileNameWithoutExtension(fullFileName);
        }

    }
}
