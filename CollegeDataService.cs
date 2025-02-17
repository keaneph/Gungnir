// CollegeDataService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CollegeDataService
{
    private readonly string _filePath;

    public string CurrentUser { get; set; } = "Admin";

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
            //Use streamwriter to ensure it is handled correctly.
            using (StreamWriter sw = File.AppendText(_filePath))
            {
                sw.WriteLine(college.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding college: {ex.Message}");
            // Log the exception or handle it in another way.  Consider a more user-friendly error message.
            throw; //Re-throw the exception so that we know there is a problem.
        }
    }

    public void UpdateCollege(College oldCollege, College newCollege)
    {
        List<College> colleges = GetAllColleges();
        bool replaced = false;
        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (College college in colleges)
                {
                    if (college.Name == oldCollege.Name && college.Code == oldCollege.Code)
                    {
                        newCollege.DateTime = DateTime.Now;
                        newCollege.User = CurrentUser;
                        sw.WriteLine(newCollege.ToString());
                        replaced = true;
                    }
                    else
                    {
                        sw.WriteLine(college.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating college: {ex.Message}");
            throw;
        }

        if (!replaced)
        {
            Console.WriteLine($"College to update not found: {oldCollege.Name} - {oldCollege.Code}");
        }
    }

    public void DeleteCollege(College collegeToDelete)
    {
        List<College> colleges = GetAllColleges();
        bool removed = false;
        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (College college in colleges)
                {
                    if (college.Name == collegeToDelete.Name && college.Code == collegeToDelete.Code)
                    {
                        removed = true;
                    }
                    else
                    {
                        sw.WriteLine(college.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting college: {ex.Message}");
            throw;
        }

        if (!removed)
        {
            Console.WriteLine($"College to delete not found: {collegeToDelete.Name} - {collegeToDelete.Code}");
        }
    }
}