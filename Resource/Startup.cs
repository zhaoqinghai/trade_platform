using Resource.Icons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resource
{
    public class Startup
    {
        public static void Init()
        {
            PackIconDataFactory.GetIconDatas();
            ThemeSelector.Default.SetDefaultTheme(ThemeType.Amber);
            ThemeSelector.Default.SetDefaultTheme(false);
        }
    }
}
