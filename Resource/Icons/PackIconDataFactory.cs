using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resource.Icons
{
    public static class PackIconDataFactory
    {
        public static Dictionary<PackIconState, Dictionary<PackIconKind, string>> DataFactoryDict;

        public static Dictionary<PackIconKind, string> DefaultCurrent;

        public static void GetIconDatas()
        {
            string json = string.Empty;
            using (StreamReader sr = new StreamReader("Icons/IconPath.json"))
            {
                json = sr.ReadToEnd();
            }
            DataFactoryDict = JsonConvert.DeserializeObject<Dictionary<PackIconState, Dictionary<PackIconKind, string>>>(json);
            DefaultCurrent = DataFactoryDict[default(PackIconState)];
        }

        public static void SetCurrenDict(PackIconState state)
        {
            DefaultCurrent = DataFactoryDict[state];
        }
    }
}
