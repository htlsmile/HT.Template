using System;
using System.Collections.Concurrent;
using System.IO;
using System.Xml;

namespace HT.Template.BackEnd
{
    public class XMLDoc
    {
        public string FileName { get; }
        public string FilePath { get; }
        private readonly ConcurrentDictionary<string, string> dict = new ConcurrentDictionary<string, string>();

        public XMLDoc(string name)
        {
            FileName = name.EndsWith(".xml") ? name : $"{name}.xml";
            FilePath = Path.Combine(AppContext.BaseDirectory, FileName);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(FilePath);
            foreach (XmlElement element in xmlDoc.SelectNodes("//member"))
            {
                switch (element.GetAttribute("name").Split(':')[0])
                {
                    case "T":
                        dict.TryAdd(element.GetAttribute("name").Split(':')[1], element.InnerText.Trim());
                        break;
                    case "M":
                        dict.TryAdd(element.GetAttribute("name").Split(':', '(')[1], element.FirstChild.InnerText.Trim());
                        break;
                    default:
                        break;
                }
            }
        }

        public static XMLDoc Current { get; } = new XMLDoc(Config.AssemblyName);

        public string GetComment(string fullName) => dict.TryGetValue(fullName, out var value) ? value : string.Empty;
    }
}
