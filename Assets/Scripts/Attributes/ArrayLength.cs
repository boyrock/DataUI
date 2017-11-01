using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataUI
{
    [AttributeUsage(AttributeTargets.Field)]
    class ArrayLength : Attribute
    {
        internal readonly int Length;
        internal ArrayLength(int length)
        {
            this.Length = length;
        }
    }
}
