using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sis_app.Models;

namespace sis_app.Services
{
    // service class to handle all student data operations (crud operations)
    public class StudentDataService
    {
        // path to the csv file storing student data
        private readonly string _filePath;

        // current user performing operations (defaults to "Admin")
        public string CurrentUser { get; set; } = "Admin";

        // constructor initializes service with file path
        public StudentDataService(string filePath)
        {
            _filePath = filePath;
        }

        // retrieves all students from the csv file
        public List<Student> GetAllStudents()
        {
            List<Student> students = new List<Student>();

            // return empty list if file doesn't exist
            if (!File.Exists(_filePath))
            {
                return students;
            }

            try
            {
                // read each line and convert to student object
                foreach (string line in File.ReadLines(_filePath))
                {
                    students.Add(Student.FromCsv(line));
                }
            }
            catch (Exception ex)
            {
                // log error but don't crash the application
                Console.WriteLine($"Error reading student data: {ex.Message}");
            }

            return students;
        }

        // adds a new student to the csv file
        public void AddStudent(Student student)
        {
            try
            {
                // append new student to file using streamwriter
                // using statement ensures proper resource disposal
                using (StreamWriter sw = File.AppendText(_filePath))
                {
                    sw.WriteLine(student.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding student: {ex.Message}");
                throw; // rethrow to notify caller of failure
            }
        }

        // updates existing student information
        public void UpdateStudent(Student oldStudent, Student newStudent)
        {
            // get current list of students
            List<Student> students = GetAllStudents();
            bool replaced = false;

            try
            {
                // rewrite entire file with updated student data
                using (StreamWriter sw = new StreamWriter(_filePath))
                {
                    foreach (Student student in students)
                    {
                        // if match found by id number, write new student data
                        if (student.IDNumber == oldStudent.IDNumber)
                        {
                            // update audit fields
                            newStudent.DateTime = DateTime.Now;
                            newStudent.User = CurrentUser;
                            sw.WriteLine(newStudent.ToString());
                            replaced = true;
                        }
                        else
                        {
                            // keep existing student data
                            sw.WriteLine(student.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating student: {ex.Message}");
                throw; // rethrow to handle in UI
            }

            // log if student wasn't found
            if (!replaced)
            {
                Console.WriteLine($"Student to update not found: {oldStudent.IDNumber}");
            }
        }

        // removes a student from the csv file
        public void DeleteStudent(Student studentToDelete)
        {
            // get current list of students
            List<Student> students = GetAllStudents();
            bool removed = false;

            try
            {
                // rewrite file excluding deleted student
                using (StreamWriter sw = new StreamWriter(_filePath))
                {
                    foreach (Student student in students)
                    {
                        // skip the student to be deleted
                        if (student.IDNumber == studentToDelete.IDNumber)
                        {
                            removed = true;
                        }
                        else
                        {
                            // keep other students
                            sw.WriteLine(student.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting student: {ex.Message}");
                throw; // rethrow to handle in UI
            }

            // log if student wasn't found
            if (!removed)
            {
                Console.WriteLine($"Student to delete not found: {studentToDelete.IDNumber}");
            }
        }
    }
}