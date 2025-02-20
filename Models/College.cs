using System;

namespace sis_app.Models
{
    public class College
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime DateTime { get; set; }
        public string User { get; set; }

        public override string ToString()
        {
            return $"{Name},{Code},{DateTime},{User}";
        }

        public static College FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return new College
            {
                Name = values[0],
                Code = values[1],
                DateTime = DateTime.Parse(values[2]),
                User = values[3]
            };
        }
    }
}