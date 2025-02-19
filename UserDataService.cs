using System;
using System.IO;
using System.Linq;

namespace glaive
{
    public class UserDataService
    {
        private readonly string _filePath;

        public UserDataService(string filePath)
        {
            _filePath = filePath;
            EnsureFileExists();
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "Username,Password\n");
            }
        }

        public bool ValidateUser(string username, string password)
        {
            try
            {
                var lines = File.ReadAllLines(_filePath).Skip(1);
                return lines.Any(line =>
                {
                    var parts = line.Split(',');
                    return parts[0] == username && parts[1] == password;
                });
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RegisterUser(string username, string password)
        {
            try
            {
                File.AppendAllText(_filePath, $"{username},{password}\n");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UsernameExists(string username)
        {
            try
            {
                var lines = File.ReadAllLines(_filePath).Skip(1);
                return lines.Any(line => line.Split(',')[0] == username);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}