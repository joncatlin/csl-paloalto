using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Start
{
    public class Members
    {
        public Members()
        {
        }

        public Members(string[] member)
        {
            Member = member;
        }
        [XmlElement("Member")]
        public string[] Member { get; set; }
    }


    public class Entry
    {
        public Entry()
        {
        }

        public Entry(string name, Members to, Members from, Members source)
        {
            Name = name;
            To = to;
            From = from;
            Source = source;
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        public Members To { get; set; }
        public Members From { get; set; }
        public Members Source { get; set; }
    }


    class Program
    {
        private static readonly int TO = 8;
        private static readonly int FROM = 4;
        private static readonly int NAME = 1;
        private static readonly int SOURCE = 5;

        static void Main(string[] args)
        {
            string fileName = null;
            string delimiter = null;

            // Assumptions:
            // The first argument is the na,e of the file to read
            // The file is using a delimiter specified by the second argument
            if (args[0] == "")
            {
                Console.WriteLine("First argument must be a valid filename, including path.");
            } else
            {
                fileName = args[0];
            }

            if (args[1] == "")
            {
                Console.WriteLine("Second argument must be the delimiter used in the file to separate fields.");
            } else
            {
                delimiter = args[1];
            }

            // Process the file and get all of the rules
            var rules = ProcessFile(fileName, delimiter);

            // Determine the output directory of the file
            var directoryName = Path.GetDirectoryName(fileName);

            // Output the rules in XML format
            OutputXML(rules, directoryName);
        }


        private static List<Entry> ProcessFile(string fileName, string delimiter)
        {
            bool firstLine = true;
            List<Entry> rules = new List<Entry>();
            string r = File.ReadAllText(fileName, Encoding.ASCII);
            string[] lines = r.Split("\r\n");

            foreach (string line in lines) {
                string[] elements = line.Split(delimiter);
                if (firstLine)
                {
                    // The first line of the file should contain headings. Save them minus the first few columns
                    // which contain none metric information.
                    firstLine = false;
                }
                else
                {
                    if (elements.Length == 16)
                    {
                        // For each line create an object with the data retrieved
                        var rule = new Entry(elements[NAME], 
                            getMembersLF(elements[TO].Trim('"')), 
                            getMembersLF(elements[FROM].Trim('"')), 
                            getMembersLF(elements[SOURCE].Trim('"')));
                        rules.Add(rule);
                    }
                    else
                    {
                        Console.WriteLine("Line in file is missing parameters, element 0 = {0}.", elements[0]);
                    }
                }
            }

            return rules;
        }


        private static Members getMembersLF(string data)
        {
            string[] elements = data.Split("\n");
            return new Members(elements);
        }


        private static Members getMembersSP(string data)
        {
            string[] elements = data.Split(" ");
            return new Members(elements);
        }


        private static void OutputXML(List<Entry> rules, string path)
        {
            XmlSerializer writer =
                new XmlSerializer(typeof(Entry));
            using (TextWriter file = new StreamWriter(path + "\\rules.xml"))
            {
                foreach (Entry rule in rules)
                {
                    writer.Serialize(file, rule);
                }
            }
        }
    }
}
