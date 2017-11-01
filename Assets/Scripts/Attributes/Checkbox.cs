using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataUI
{
    [AttributeUsage(AttributeTargets.Field)]
    class Checkbox : Attribute
    {
        internal readonly string[] Items;
        internal Checkbox(string items)
        {
            this.Items = items.Split(',');
        }
    }
}
