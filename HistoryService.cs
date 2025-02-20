using sis_app;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
public class HistoryService
{
    private readonly string _filePath = "history.csv";
    private ObservableCollection<HistoryEntry> _history;

    // Keep the original event declaration
    public event EventHandler<HistoryEntry> NewEntryAdded;

    public HistoryService(bool clearExistingHistory = false) // Add this parameter
    {
        _history = new ObservableCollection<HistoryEntry>();

        // Clear existing history if requested
        if (clearExistingHistory && File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }

        LoadHistory();
    }


    private void LoadHistory()
    {
        if (!File.Exists(_filePath))
        {
            File.Create(_filePath).Close();
            return;
        }

        var entries = File.ReadAllLines(_filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(HistoryEntry.FromCsv);

        _history = new ObservableCollection<HistoryEntry>(entries);
    }

    public void AddEntry(string user, string action, string details)
    {
        var entry = new HistoryEntry
        {
            Timestamp = DateTime.Now,
            User = user,
            Action = action,
            Details = details
        };

        _history.Add(entry);

        try
        {
            File.AppendAllText(_filePath, entry.ToString() + Environment.NewLine);
            // Pass the entry directly
            NewEntryAdded?.Invoke(this, entry);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving history: {ex.Message}", "Error");
        }
    }

    public ObservableCollection<HistoryEntry> GetHistory()
    {
        return _history;
    }
}