using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Start
{
    public class Security
    {
        public Security()
        {
        }

        public Security(List<Entry> rules)
        {
            Rules = rules;
        }

        public List<Entry> Rules { get; set; }
    }


    public class Address
    {
        public Address()
        {
        }

        public Address(List<AddressEntry> addresses)
        {
            Addresses = addresses;
        }

        public List<AddressEntry> Addresses { get; set; }
    }


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

        public Entry(string name, 
            Members to, 
            Members from, 
            Members source, 
            Members destination,
            Members sourceUser,
            Members application,
            Members service,
            Members hipProfiles,
            Members tag,
            Members action,
            Members ruleType
            )
        {
            Name = name;
            To = to;
            From = from;
            Source = source;
            Destination = destination;
            SourceUser = sourceUser;
            Application = application;
            Service = service;
            HipProfiles = hipProfiles;
            Tag = tag;
            Action = action;
            RuleType = ruleType;

            LogSetting = "AlienVault";
            Category = "any";
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        public Members To { get; set; }
        public Members From { get; set; }
        public Members Source { get; set; }
        public Members Destination { get; set; }
        [XmlElement("source-user")]
        public Members SourceUser { get; set; }
        public string Category { get; set; }
        public Members Application { get; set; }
        public Members Service { get; set; }
        [XmlElement("hip-profiles")]
        public Members HipProfiles{ get; set; }
        public Members Tag { get; set; }
        public Members Action { get; set; }
        [XmlElement("rule-type")]
        public Members RuleType { get; set; }
        [XmlElement("log-setting")]
        public string LogSetting { get; set; }
    }


    public class AddressEntry
    {
        public AddressEntry()
        {
        }

        public AddressEntry(string name,
            Members tag,
            string description,
            string fqdn,
            string ipNetmask,
            string ipRange
            )
        {
            Name = name;
            Tag = tag;
            Description = description;
            Fqdn = fqdn;
            IpNetmask = ipNetmask;
            IpRange = ipRange;
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        public Members Tag { get; set; }
        public string Description { get; set; }
        public string Fqdn { get; set; }
        [XmlElement("ip-netmask")]
        public string IpNetmask { get; set; }
        [XmlElement("ip-range")]
        public string IpRange { get; set; }

        public bool ShouldSerializeTag()
        {
            return Tag.Member.Length == 0;
        }

        public bool ShouldSerializeDescription()
        {
            return !string.IsNullOrWhiteSpace(this.Description);
        }
    }


    class Program
    {
        // Policy Constants
        private static readonly int TO = 8;
        private static readonly int FROM = 4;
        private static readonly int NAME = 1;
        private static readonly int SOURCE = 5;
        private static readonly int DESTINATION = 9;
        private static readonly int SOURCE_USER = 6;
        private static readonly int APPLICATION = 11;
        private static readonly int SERVICE = 12;
        private static readonly int HIP_PROFILES = 7;
        private static readonly int TAG = 2;            // Needs special processing combine with ,
        private static readonly int ACTION = 13;
        private static readonly int RULE_TYPE = 3;

        // Object constants
        private static readonly int PA_NAME = 2;
        private static readonly int PROCESS = 1;
        private static readonly int PA_TAGS = 3;
        private static readonly int PA_ADDRESS = 5;
        private static readonly int DNS_ENTRY = 6;
        private static readonly int DESCRIPTION = 7;

        static void Main(string[] args)
        {
            string policyFileName = null;
            string objectFileName = null;
            string delimiter = null;

            // Assumptions:
            // The first argument is the na,e of the file to read
            // The file is using a delimiter specified by the second argument
            if (args[0] == "")
            {
                Console.WriteLine("First argument must be a valid filename, including path, for the Policies specfications.");
            } else
            {
                policyFileName = args[0];
            }

            if (args[1] == "")
            {
                Console.WriteLine("Second argument must be a valid filename, including path, for the Object specifications.");
            }
            else
            {
                objectFileName = args[1];
            }

            if (args[2] == "")
            {
                Console.WriteLine("Second argument must be the delimiter used in the file to separate fields.");
            } else
            {
                delimiter = args[2];
            }

            // Process the policy file and get all of the rules
            var rules = ProcessPolicyFile(policyFileName, delimiter);
            var security = new Security(rules);

            // Process the object file and get all of the rules
            var objects = ProcessObjectFile(objectFileName, delimiter);
            var address = new Address(objects);

            // Determine the output directory of the file
            var directoryName = Path.GetDirectoryName(policyFileName);

            // Output the rules in XML format
            OutputXML(security, address, directoryName);
        }


        private static List<Entry> ProcessPolicyFile(string fileName, string delimiter)
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
                            getMembersLF(elements[SOURCE].Trim('"')),
                            getMembersLF(elements[DESTINATION].Trim('"')),
                            getMembersLF(elements[SOURCE_USER].Trim('"')),
                            getMembersLF(elements[APPLICATION].Trim('"')),
                            getMembersLF(elements[SERVICE].Trim('"')),
                            getMembersLF(elements[HIP_PROFILES].Trim('"')),
                            getMembersConvert(elements[TAG].Trim('"')),
                            getMembersLF(elements[ACTION].Trim('"')),
                            getMembersLF(elements[RULE_TYPE].Trim('"'))
                            );
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


        private static Members getMembersConvert(string data)
        {
            string[] elements = new string[1];
            // Convert spaces to commas and LF to commas
            elements[0] = data.Replace(' ', ',').Replace('\n', ',');
            return new Members(elements);
        }


        private static void OutputXML(Security security, Address address, string path)
        {
            XmlSerializer writer =
                new XmlSerializer(typeof(Security));
            using (TextWriter file = new StreamWriter(path + "\\policies.xml"))
            {
                writer.Serialize(file, security);
            }
            writer =
                new XmlSerializer(typeof(Address));
            using (TextWriter file = new StreamWriter(path + "\\addresses.xml"))
            {
                writer.Serialize(file, address);
            }
        }

        private static List<AddressEntry> ProcessObjectFile(string fileName, string delimiter)
        {
            bool firstLine = true;
            List<AddressEntry> objects = new List<AddressEntry>();
            string r = File.ReadAllText(fileName, Encoding.ASCII);
            string[] lines = r.Split("\r\n");

            foreach (string line in lines)
            {
                string[] elements = line.Split(delimiter);
                if (firstLine)
                {
                    // The first line of the file should contain headings. Save them minus the first few columns
                    // which contain none metric information.
                    firstLine = false;
                }
                else
                {
                    if (elements.Length == 8)
                    {
                        var name = elements[PA_NAME];
                        var process = elements[PROCESS];
                        var tag = getMembersConvert(elements[PA_TAGS]);
                        var fqdn = elements[DNS_ENTRY];
                        var description = elements[DESCRIPTION];
                        var ipNetmask = elements[PA_ADDRESS];
                        var ipRange = "";

                        // Rules for selecting which columns to use
                        if (!process.ToLower().Equals("y")) break;

                        if (fqdn.Equals(""))
                        {
                            // find this is an IP address or a range
                            var expr = @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}";
                            MatchCollection mc = Regex.Matches(ipNetmask, expr);

                            switch (mc.Count)
                            {
                                case 0:
                                    // No IP's found, treat as a fqdn
                                    fqdn = ipNetmask;
                                    ipNetmask = ipRange = null;
                                    break;
                                case 1:
                                    // Treat as the IP address and erase the other fields
                                    fqdn = ipRange = null;
                                    break;
                                case 2: // Treat as a range
                                    ipRange = ipNetmask.Replace(' ', '-');
                                    fqdn = ipNetmask = null;
                                    break;
                                default:
                                    Console.WriteLine("FQDN is blank but did not find either a single IP, IP Range or text for fqdn, element 0 = {0}.", elements[0]);
                                    break;
                            }
                        } else
                        {
                            // do not use the PA_ADDRESS
                            ipNetmask = ipRange = null;

                            // Add concordservicing.com to the value
                            fqdn += ".concordservicing.com";
                        }
                        // For each line create an object with the data retrieved
                        var entry = new AddressEntry(name, tag, description, fqdn, ipNetmask, ipRange);
                        objects.Add(entry);
                    }
                    else
                    {
                        Console.WriteLine("Line in file is missing parameters, element 0 = {0}.", elements[0]);
                    }
                }
            }

            return objects;
        }



    }
}
