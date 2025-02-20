using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sis_app
{
    public class HistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }

        public override string ToString()
        {
            return $"{Timestamp},{User},{Action},{Details}";
        }

        public static HistoryEntry FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return new HistoryEntry
            {
                Timestamp = DateTime.Parse(values[0]),
                User = values[1],
                Action = values[2],
                Details = values[3]
            };
        }
    }
}
