// College.cs
using System;

public class College
{
    public string Name { get; set; }
    public string Code { get; set; }
    public DateTime DateTime { get; set; } // New property
    public string User { get; set; } // New property

    public override string ToString()
    {
        return $"{Name},{Code},{DateTime},{User}"; // For CSV serialization
    }

    public static College FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        return new College
        {
            Name = values[0],
            Code = values[1],
            DateTime = DateTime.Parse(values[2]), // Parse DateTime from string
            User = values[3]
        };
    }
}