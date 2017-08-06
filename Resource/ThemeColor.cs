using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resource
{
    public class ThemeColor
    {
        public ThemeColor(string name, IEnumerable<TupleColor> primaryColors, IEnumerable<TupleColor> accentColors)
        {
            if (string.IsNullOrEmpty(name) || primaryColors == null || accentColors == null)
                throw new ArgumentNullException();

            PrimaryColors = primaryColors;
            AccentColors = accentColors;
            Name = name;
            
        }

        public string Name { get; }

        public IEnumerable<TupleColor> PrimaryColors { get; }

        public IEnumerable<TupleColor> AccentColors { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
