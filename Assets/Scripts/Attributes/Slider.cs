using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataUI
{
    [AttributeUsage(AttributeTargets.Field)]
    class Slider : Attribute
    {
        internal readonly float range_from;
        internal readonly float range_to;
        internal Slider(float from, float to)
        {
            this.range_from = from;
            this.range_to = to;
        }
    }
}
