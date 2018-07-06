using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracking.objects
{
    class settingItem
    {
        public string Setting { get; set; }
        public object Value { get; set; }

        public settingItem(string setting, object value)
        {
            this.Setting = setting;
            this.Value = value;
        }
    }
}
