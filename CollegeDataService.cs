// CollegeDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CollegeDataService
{
    private readonly string _filePath;

    public CollegeDataService(string filePath)
    {
        _filePath = filePath;
    }

    public List<College> GetAllColleges()
    {
        List<College> colleges = new List<College>();

        if (!File.Exists(_filePath))
        {
            return colleges; // Return empty list if file doesn't exist
        }

        try
        {
            foreach (string line in File.ReadLines(_filePath))
            {
                colleges.Add(College.FromCsv(line));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading college data: {ex.Message}");
            // Log the exception or handle it in another way.
        }

        return colleges;
    }

    public void AddCollege(College college)
    {
        try
        {
            File.AppendAllText(_filePath, college.ToString() + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding college: {ex.Message}");
            // Log the exception or handle it in another way.  Consider a more user-friendly error message.
        }
    }
}
