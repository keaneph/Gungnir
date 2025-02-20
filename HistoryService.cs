using sis_app;
using System.Collections.ObjectModel;

public class HistoryService
{
    private readonly string _filePath = "history.csv";
    private ObservableCollection<HistoryEntry> _history;

    public HistoryService()
    {
        _history = new ObservableCollection<HistoryEntry>();
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving history: {ex.Message}");
        }
    }

    public ObservableCollection<HistoryEntry> GetHistory()
    {
        return _history;
    }
}