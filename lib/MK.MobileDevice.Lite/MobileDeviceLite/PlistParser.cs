/*
MK PList Parser
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace MK.Plist
{
    public class PList : Dictionary<string, dynamic>
    {
        public static string StripControlChars(string arg)
        {
            char[] arrForm = arg.ToCharArray();
            StringBuilder buffer = new StringBuilder(arg.Length);//This many chars at most

            foreach (char ch in arrForm)
                if (!Char.IsControl(ch)) buffer.Append(ch);//Only add to buffer if not a control char

            return buffer.ToString();
        }

        static string StripExtended(string arg)
        {
            StringBuilder buffer = new StringBuilder(arg.Length); //Max length
            foreach (char ch in arg)
            {
                UInt16 num = Convert.ToUInt16(ch);//In .NET, chars are UTF-16
                //The basic characters have the same code points as ASCII, and the extended characters are bigger
                if ((num >= 32u) && (num <= 126u)) buffer.Append(ch);
            }
            return buffer.ToString();
        }

        static bool ignoreErr = false;
    	//private XDocument _plist;
        public PList()
        {
        }
        
        public PList(XDocument PList, bool ignoreErrors)
        {
        	Clear();

        	XDocument doc = PList;
        	XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        public PList(string file)
        {
            Load(file);
        }

        public void Load(string file)
        {
            Clear();

            XDocument doc = XDocument.Load(file);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        private void Parse(PList dict, IEnumerable<XElement> elements)
        {
            for (int i = 0; i < elements.Count(); i += 2)
            {
                XElement key = elements.ElementAt(i);
                XElement val = elements.ElementAt(i + 1);

                dict[key.Value] = ParseValue(val, ignoreErr);
            }
        }

        private List<dynamic> ParseArray(IEnumerable<XElement> elements)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (XElement e in elements)
            {
                dynamic one = ParseValue(e, ignoreErr);
                list.Add(one);
            }

            return list;
        }

        private dynamic ParseValue(XElement val, bool ignoreErrors)
        {
            switch (val.Name.ToString())
            {
                case "string":
                    return val.Value;
                case "data":
                    return val.Value;
                case "date":
                    return val.Value;
                case "integer":
                    return long.Parse(val.Value);
                case "real":
                    return float.Parse(val.Value);
                case "true":
                    return true;
                case "false":
                    return false;
                case "dict":
                    PList plist = new PList();
                    Parse(plist, val.Elements());
                    return plist;
                case "array":
                    List<dynamic> list = ParseArray(val.Elements());
                    return list;
                default:
                    if (!ignoreErrors)
                    	throw new ArgumentException("Unsupported: "+val.Name.ToString());
                    else
                    {
                        string rawVal = val.Value;
                        string cleanVal = StripControlChars(rawVal);
                        return cleanVal;
                    }
            }
        }
    }
}