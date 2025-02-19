using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class StudentDataService
{
    private readonly string _filePath;

    public string CurrentUser { get; set; } = "Admin"; //Set a default value

    public StudentDataService(string filePath)
    {
        _filePath = filePath;
    }

    public List<Student> GetAllStudents()
    {
        List<Student> students = new List<Student>();
        if (!File.Exists(_filePath))
        {
            return students;
        }

        try
        {
            foreach (string line in File.ReadLines(_filePath))
            {

                students.Add(Student.FromCsv(line));

            }


        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading student data: {ex.Message}");
        }

        return students;
    }




    public void AddStudent(Student student)
    {
        try
        {
            using (StreamWriter sw = File.AppendText(_filePath))
            {
                sw.WriteLine(student.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding student: {ex.Message}");
            throw; //Rethrow to handle it in UI
        }
    }




    // Implement UpdateStudent and DeleteStudent methods as needed (similar to CollegeDataService)

    public void UpdateStudent(Student oldStudent, Student newStudent)
    {
        //Implementation for updating student data in the CSV file
        List<Student> students = GetAllStudents();
        bool replaced = false;
        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (Student student in students)
                {
                    if (student.IDNumber == oldStudent.IDNumber)
                    {
                        newStudent.DateTime = DateTime.Now; //Update datetime
                        newStudent.User = CurrentUser; //Update user who modified it
                        sw.WriteLine(newStudent.ToString());
                        replaced = true;
                    }
                    else
                    {
                        sw.WriteLine(student.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating student: {ex.Message}");
            throw; //Rethrow to handle in UI
        }


        if (!replaced)
        {
            Console.WriteLine($"Student to update not found: {oldStudent.IDNumber}");
        }

    }

    public void DeleteStudent(Student studentToDelete)
    {
        //Implementation for deleting student data in CSV file.
        List<Student> students = GetAllStudents();
        bool removed = false;

        try
        {
            using (StreamWriter sw = new StreamWriter(_filePath))
            {
                foreach (Student student in students)
                {

                    if (student.IDNumber == studentToDelete.IDNumber)
                    {

                        removed = true;


                    }
                    else
                    {

                        sw.WriteLine(student.ToString());
                    }


                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting student: {ex.Message}");
            throw; //Rethrow to handle it in UI
        }

        if (!removed)
        {
            Console.WriteLine($"Student to delete not found: {studentToDelete.IDNumber}");
        }


    }
}