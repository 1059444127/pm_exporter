using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Pm.MsSql
{
    public class CustomMetricSett
    {
        [XmlAttribute()]
        public string Type;

        [XmlAttribute]
        public string Name;

        public string Help;

        // [XmlAttribute]
        // public string Fields;

        public string Sql;

        [XmlAttribute()]
        public bool IsEnabled;
    }
}
