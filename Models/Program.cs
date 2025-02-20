using System;

namespace sis_app.Models
{
    public class Program
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string CollegeCode { get; set; }
        public DateTime DateTime { get; set; }
        public string User { get; set; }

        public override string ToString()
        {
            return $"{Name},{Code},{CollegeCode},{DateTime},{User}";
        }

        public static Program FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return new Program
            {
                Name = values[0],
                Code = values[1],
                CollegeCode = values[2],
                DateTime = DateTime.Parse(values[3]),
                User = values[4]
            };
        }
    }
}