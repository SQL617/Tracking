using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Tracking
{
    public class IDN
    {
        public IDN()
        {

        }
        public List<System.Windows.Point> pList = new List<System.Windows.Point>();
        public string name;
        public string timeChannel { get; set; }

        public double scaleX { get; set; }
        public double scaleY { get; set; }

        public void addPointToList(System.Windows.Point p)
        {
            pList.Add(p);
        }

    }
}
