using System;

namespace sis_app.Models
{
    public class Student
    {
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int YearLevel { get; set; }
        public string Gender { get; set; }
        public string ProgramCode { get; set; }
        public string CollegeCode { get; set; }
        public DateTime DateTime { get; set; }
        public string User { get; set; }

        public override string ToString()
        {
            return $"{IDNumber},{FirstName},{LastName},{YearLevel},{Gender},{ProgramCode},{CollegeCode},{DateTime},{User}";
        }

        public static Student FromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            return new Student
            {
                IDNumber = values[0],
                FirstName = values[1],
                LastName = values[2],
                YearLevel = int.Parse(values[3]),
                Gender = values[4],
                ProgramCode = values[5],
                CollegeCode = values[6],
                DateTime = DateTime.Parse(values[7]),
                User = values[8]
            };
        }
    }
}