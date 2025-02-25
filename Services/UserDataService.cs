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
        private readonly string _filePath;

        /// <summary>
        /// Constructor initializes service and ensures credential file exists
        /// </summary>
        /// <param name="filePath">Path to user credentials file</param>
        public UserDataService(string filePath)
        {
            _filePath = filePath;
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
                var lines = File.ReadAllLines(_filePath).Skip(1);
                return lines.Any(line =>
                {
                    var parts = line.Split(',');
                    return parts[0] == username && parts[1] == password;
                });
            }
            catch (Exception)
            {
                // return false on any file access errors
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
                File.AppendAllText(_filePath, $"{username},{password}\n");
                return true;
            }
            catch (Exception)
            {
                // return false if registration fails
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
                var lines = File.ReadAllLines(_filePath).Skip(1);
                return lines.Any(line => line.Split(',')[0] == username);
            }
            catch (Exception)
            {
                // return false on any file access errors
                return false;
            }
        }
    }
}