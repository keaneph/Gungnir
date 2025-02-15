// College.cs
public class College
{
    public string Name { get; set; }
    public string Code { get; set; }

    public override string ToString()
    {
        return $"{Name},{Code}"; // For CSV serialization
    }

    public static College FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        return new College
        {
            Name = values[0],
            Code = values[1]
        };
    }
}