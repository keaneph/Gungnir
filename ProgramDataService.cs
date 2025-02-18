// ProgramDataService.cs (very similar to CollegeDataService.cs)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ProgramDataService
{
    // ... (Implement methods like GetAllPrograms, AddProgram, UpdateProgram, DeleteProgram) ...
    //Same code with CollegeDataService.cs but replace college with program and colleges with programs. 
    //Also replace colleges.csv with programs.csv

    private readonly string _filePath;

    public string CurrentUser { get; set; } = "Admin";

    public ProgramDataService(string filePath)
    {
        _filePath = filePath;
    }

    public List<Program> GetAllPrograms()
    {
        List<Program> programs = new List<Program>();

        if (!File.Exists(_filePath))
        {
            return programs; // Return empty list if file doesn't exist
        }

        try
        {
            foreach (string line in File.ReadLines(_filePath))
            {
                programs.Add(Program.FromCsv(line));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading programs data: {ex.Message}");
            // Log the exception or handle it in another way.
        }

        return programs;
    }

    public void AddProgram(Program program)
    {
        try
        {
            //Use streamwriter to ensure it is handled correctly.
            using (StreamWriter sw = File.AppendText(_filePath))
            {
                sw.WriteLine(program.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding program: {ex.Message}");
            // Log the exception or handle it in another way.  Consider a more user-friendly error message.
            throw; //Re-throw the exception so that we know there is a problem.
        }
    }

    public void UpdateProgram(Program oldProgram, Program newProgram)
    {
        List<Program> programs = GetAllPrograms();
        bool replaced = false;
        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (Program program in programs)
                {
                    if (program.Name == oldProgram.Name && program.Code == oldProgram.Code)
                    {
                        newProgram.DateTime = DateTime.Now;
                        newProgram.User = CurrentUser;
                        sw.WriteLine(newProgram.ToString());
                        replaced = true;
                    }
                    else
                    {
                        sw.WriteLine(program.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating program: {ex.Message}");
            throw;
        }

        if (!replaced)
        {
            Console.WriteLine($"Program to update not found: {oldProgram.Name} - {oldProgram.Code}");
        }
    }

    public void DeleteProgram(Program programToDelete)
    {
        List<Program> programs = GetAllPrograms();
        bool removed = false;
        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (Program program in programs)
                {
                    if (program.Name == programToDelete.Name && program.Code == programToDelete.Code)
                    {
                        removed = true;
                    }
                    else
                    {
                        sw.WriteLine(program.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting program: {ex.Message}");
            throw;
        }

        if (!removed)
        {
            Console.WriteLine($"Program to delete not found: {programToDelete.Name} - {programToDelete.Code}");
        }
    }
}