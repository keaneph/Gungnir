using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using sis_app.Models;

namespace sis_app.Services
{
    // service class to handle all college data operations (crud operations)
    public class CollegeDataService
    {
        // path to the csv file storing college data
        private readonly string _filePath;

        // current user performing operations (defaults to "Admin")
        public string CurrentUser { get; set; } = "Admin";

        // constructor initializes service with file path
        public CollegeDataService(string filePath)
        {
            _filePath = filePath;
        }

        // retrieves all colleges from the csv file
        public List<College> GetAllColleges()
        {
            List<College> colleges = new List<College>();

            // return empty list if file doesn't exist
            if (!File.Exists(_filePath))
            {
                return colleges;
            }

            try
            {
                // read each line and convert to college object
                foreach (string line in File.ReadLines(_filePath))
                {
                    colleges.Add(College.FromCsv(line));
                }
            }
            catch (Exception ex)
            {
                // log error but don't crash the application
                Console.WriteLine($"Error reading college data: {ex.Message}");
            }

            return colleges;
        }

        // adds a new college to the csv file
        public void AddCollege(College college)
        {
            try
            {
                // append new college to file using streamwriter
                // using statement ensures proper resource disposal
                using (StreamWriter sw = File.AppendText(_filePath))
                {
                    sw.WriteLine(college.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding college: {ex.Message}");
                throw; // rethrow to notify caller of failure
            }
        }

        // updates existing college information
        public void UpdateCollege(College oldCollege, College newCollege)
        {
            // get current list of colleges
            List<College> colleges = GetAllColleges();
            bool replaced = false;

            try
            {
                // rewrite entire file with updated college data
                using (StreamWriter sw = new StreamWriter(_filePath))
                {
                    foreach (College college in colleges)
                    {
                        // if match found, write new college data
                        if (college.Name == oldCollege.Name && college.Code == oldCollege.Code)
                        {
                            // update audit fields
                            newCollege.DateTime = DateTime.Now;
                            newCollege.User = CurrentUser;
                            sw.WriteLine(newCollege.ToString());
                            replaced = true;
                        }
                        else
                        {
                            // keep existing college data
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

            // log if college wasn't found
            if (!replaced)
            {
                Console.WriteLine($"College to update not found: {oldCollege.Name} - {oldCollege.Code}");
            }
        }

        // removes a college from the csv file
        public void DeleteCollege(College collegeToDelete)
        {
            // get current list of colleges
            List<College> colleges = GetAllColleges();
            bool removed = false;

            try
            {
                // rewrite file excluding deleted college
                using (StreamWriter sw = new StreamWriter(_filePath))
                {
                    foreach (College college in colleges)
                    {
                        // skip the college to be deleted
                        if (college.Name == collegeToDelete.Name && college.Code == collegeToDelete.Code)
                        {
                            removed = true;
                        }
                        else
                        {
                            // keep other colleges
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

            // log if college wasn't found
            if (!removed)
            {
                Console.WriteLine($"College to delete not found: {collegeToDelete.Name} - {collegeToDelete.Code}");
            }
        }
    }
}