using System;
using System.IO;
using System.Linq;

namespace sis_app.Services
{
    /// <summary>
    /// Service class to handle user authentication and registration operations
    /// </summary>
    public class UserDataService
    {
        // path to csv file storing user credentials
        internal readonly string _filePath;

        /// <summary>
        /// Constructor initializes service with file path and ensures data directory exists
        /// </summary>
        /// <param name="fileName">Name of the credentials file</param>
        public UserDataService(string fileName)
        {
            // create path to Data folder in project directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Path.Combine(baseDirectory, "..\\..\\..\\");
            string dataDirectory = Path.Combine(projectDirectory, "Data");

            // create Data directory if it doesn't exist
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            // set full path to csv file
            _filePath = Path.Combine(dataDirectory, fileName);

            // ensure credentials file exists with header
            EnsureFileExists();
        }

        /// <summary>
        /// Creates credentials file if it doesn't exist
        /// Initializes with header row for CSV structure
        /// </summary>
        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
            {
                // create new file with header row
                File.WriteAllText(_filePath, "Username,Password\n");
            }
        }

        /// <summary>
        /// Validates user credentials against stored data
        /// </summary>
        /// <param name="username">User's username</param>
        /// <param name="password">User's password</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        public bool ValidateUser(string username, string password)
        {
            try
            {
                // skip header row and check credentials
                var lines = File.ReadAllLines(_filePath)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l));

                return lines.Any(line =>
                {
                    var parts = line.Split(',');
                    return parts.Length == 2 &&
                           parts[0] == username &&
                           parts[1] == password;
                });
            }
            catch (Exception ex)
            {
                // log error and return false on any file access errors
                Console.WriteLine($"Error validating user: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="username">New user's username</param>
        /// <param name="password">New user's password</param>
        /// <returns>True if registration successful, false otherwise</returns>
        public bool RegisterUser(string username, string password)
        {
            try
            {
                // append new user credentials to file
                using (StreamWriter sw = File.AppendText(_filePath))
                {
                    sw.WriteLine($"{username},{password}");
                }
                return true;
            }
            catch (Exception ex)
            {
                // log error and return false if registration fails
                Console.WriteLine($"Error registering user: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if username is already taken
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if username exists, false otherwise</returns>
        public bool UsernameExists(string username)
        {
            try
            {
                // skip header row and check for username
                var lines = File.ReadAllLines(_filePath)
                    .Skip(1)
                    .Where(l => !string.IsNullOrWhiteSpace(l));

                return lines.Any(line =>
                {
                    var parts = line.Split(',');
                    return parts.Length > 0 && parts[0] == username;
                });
            }
            catch (Exception ex)
            {
                // log error and return false on any file access errors
                Console.WriteLine($"Error checking username: {ex.Message}");
                return false;
            }
        }
    }
}