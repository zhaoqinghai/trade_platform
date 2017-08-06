using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Resource
{
    public class TupleColor
    {

        public TupleColor(string name , Color forecolor, Color backcolor)
        {
            if (string.IsNullOrEmpty(name) || forecolor == null || backcolor == null)
                throw new ArgumentNullException();
            Name = name;
            BackColor = backcolor;
            ForeColor = forecolor;
        }
        public Color BackColor { get; }

        public Color ForeColor { get; }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
