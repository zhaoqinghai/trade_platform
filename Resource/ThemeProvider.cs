using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Resources;


namespace Resource
{
    public class ThemeProvider
    {
        public ThemeProvider() : this(Assembly.GetExecutingAssembly())
        {

        }

        public ThemeProvider(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var dictionaryEntries = GetAllAssemblyResources(assembly);

            var regex = new Regex(@"^colorbox\/(?<name>[a-z]+)_(?<type>primary|accent)\.baml$");
            var match = regex.Match("colorbox/amber_primary.baml");


            ThemeColors = dictionaryEntries.Select(x => new { key = x.Key.ToString(), match = regex.Match(x.Key.ToString())}).Where(x => x.match.Success).GroupBy(x => x.match.Groups["name"].Value).Select(x => CreateTheme(x.Key, Read( x.SingleOrDefault(y => y.match.Groups["type"].Value == "primary")?.key),
                    Read( x.SingleOrDefault(y => y.match.Groups["type"].Value == "accent")?.key))).ToList().ToDictionary(x=>x.Key,x=>x.Value);

            KeyValuePair<string, ThemeColor> CreateTheme(string name, ResourceDictionary primaryDictionary, ResourceDictionary accentDictionary)
            {

                var primaryThemeColors = new List<TupleColor>();
                var accentThemeColors = new List<TupleColor>();
                if (primaryDictionary != null)
                {
                    foreach (var entry in primaryDictionary.OfType<DictionaryEntry>()
                        .OrderBy(de => de.Key)
                        .Where(de => !de.Key.ToString().EndsWith("Foreground", StringComparison.Ordinal)))
                    {
                        var colour = (Color)entry.Value;
                        var foregroundColour = (Color)
                            primaryDictionary.OfType<DictionaryEntry>()
                                .Single(de => de.Key.ToString().Equals(entry.Key.ToString() + "Foreground"))
                                .Value;

                        primaryThemeColors.Add(new TupleColor(entry.Key.ToString(), foregroundColour, colour ));
                    }
                }
                if (accentDictionary != null)
                {
                    foreach (var entry in accentDictionary.OfType<DictionaryEntry>()
                        .OrderBy(de => de.Key)
                        .Where(de => !de.Key.ToString().EndsWith("Foreground", StringComparison.Ordinal)))
                    {
                        var colour = (Color)entry.Value;
                        var foregroundColour = (Color)
                            accentDictionary.OfType<DictionaryEntry>()
                                .Single(de => de.Key.ToString().Equals(entry.Key.ToString() + "Foreground"))
                                .Value;

                        accentThemeColors.Add(new TupleColor(entry.Key.ToString(), foregroundColour,colour ));
                    }
                }
                var themeColor = new ThemeColor(name, primaryThemeColors, accentThemeColors);
                return new KeyValuePair<string, ThemeColor>(name, themeColor);
            }
            ResourceDictionary Read(string path)
            {
                if (path == null)
                    return null;

                return (ResourceDictionary)Application.LoadComponent(new Uri(
                    $"/{assemblyName};component/{path.Replace(".baml", ".xaml")}",
                    UriKind.RelativeOrAbsolute));
            }

        }

        public Dictionary<string,ThemeColor> ThemeColors { get; }

        private IList<DictionaryEntry> GetAllAssemblyResources(Assembly assembly)
        {

#if debug
            var resourcesName = assembly.GetName().Name + ".g";
            var manager = new ResourceManager(resourcesName, assembly);
            var resourceSet = manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            return resourceSet.OfType<DictionaryEntry>().ToList();
#else
            string resName = assembly.GetName().Name + ".g.resources";
            using (var stream = assembly.GetManifestResourceStream(resName))
            {
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    return reader.Cast<DictionaryEntry>().ToList();
                }
            }
#endif

        }


    }
}
