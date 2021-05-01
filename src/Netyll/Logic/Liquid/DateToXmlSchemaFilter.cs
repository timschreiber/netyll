using System;
using System.Xml;

namespace Netyll.Logic.Liquid
{
    public static class DateToXmlSchemaFilter
    {
        public static string date_to_xmlschema(DateTime input)
        {
            return XmlConvert.ToString(input, XmlDateTimeSerializationMode.Local);
        }
    }
}
