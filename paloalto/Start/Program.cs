using System;
using System.Collections.Generic;
using System.IO;

namespace Start
{
    public class Rule
    {
        public Rule()
        {
        }

        public Rule(string name, string param1, string param2, string param3)
        {
            Name = name;
            Param1 = param1;
            Param2 = param2;
            Param3 = param3;
        }

        public string Name { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
    }


    class Program
    {
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


        private static List<Rule> ProcessFile(string fileName, string delimiter)
        {
            int lines = 0;
            bool firstLine = true;
            List<Rule> rules = new List<Rule>();

            using (StreamReader sr = File.OpenText(fileName))
            {
                string s = String.Empty;

                // Read the first line for the heading information for each metric
                //var headings = ParseLine(sr.ReadLine());

                while ((s = sr.ReadLine()) != null)
                {
                    lines++;
                    string[] elements = s.Split(delimiter);
                    if (firstLine)
                    {
                        // The first line of the file should contain headings. Save them minus the first few columns
                        // which contain none metric information.
                        firstLine = false;
                    }
                    else
                    {
                        if (elements.Length == 4)
                        {
                            // For each line create an object with the data retrieved
                            var rule = new Rule(elements[0], elements[1], elements[2], elements[3]);
                            rules.Add(rule);
                        }
                        else
                        {
                            Console.WriteLine("Line in file is missing parameters, element 0 = {0}.", elements[0]);
                        }
                    }
                }
            }

            return rules;
        }


        private static void OutputXML(List<Rule> rules, string path)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Rule));

            System.IO.FileStream file = System.IO.File.Create(path + "\\rules.xml");

            foreach(Rule rule in rules)
            {
                writer.Serialize(file, rule);
            }
            file.Close();
        }
    }
}
