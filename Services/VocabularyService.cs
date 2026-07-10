using Web_Learning_Vocabulary.Models;

using System.Text.Json;

namespace Web_Learning_Vocabulary.Services;

public interface IVocabularyService
{
    Task<List<VocabularyItem>> GetAllVocabularyAsync();
    Task<VocabularyItem?> GetVocabularyByIdAsync(int id);
    Task<VocabularyItem> AddVocabularyAsync(VocabularyItem vocabulary);
    Task<VocabularyItem?> UpdateVocabularyAsync(int id, VocabularyItem vocabulary);
    Task<bool> DeleteVocabularyAsync(int id);
}

public class VocabularyService : IVocabularyService
{
    private static List<VocabularyItem> _vocabularies = new();
    private static int _nextId = 1;
    private static bool _isLoaded = false;
    private readonly string _dataPath = "Data/vocabularies.json";

    public VocabularyService()
    {
        EnsureDataLoaded();
    }

    private void EnsureDataLoaded()
    {
        if (_isLoaded) return;

        if (!Directory.Exists("Data"))
            Directory.CreateDirectory("Data");

        if (File.Exists(_dataPath))
        {
            var json = File.ReadAllText(_dataPath);
            _vocabularies = JsonSerializer.Deserialize<List<VocabularyItem>>(json) ?? new List<VocabularyItem>();
            if (_vocabularies.Any())
                _nextId = _vocabularies.Max(v => v.Id) + 1;
            else
                _nextId = 1;
        }
        else
        {
            _vocabularies = new List<VocabularyItem>
            {
                new VocabularyItem
                {
                    Id = 1,
                    EnglishWord = "Reliable",
                    VietnameseMeaning = "Đáng tin cậy",
                    Example = "He is a reliable person.",
                    WordType = "Tính từ"
                },
                new VocabularyItem
                {
                    Id = 2,
                    EnglishWord = "Persevere",
                    VietnameseMeaning = "Kiên trì, bền chí",
                    Example = "You must persevere to achieve your goals.",
                    WordType = "Động từ"
                },
                new VocabularyItem
                {
                    Id = 3,
                    EnglishWord = "Innovative",
                    VietnameseMeaning = "Sáng tạo, tân tiến",
                    Example = "The company has an innovative approach.",
                    WordType = "Tính từ"
                }
            };
            _nextId = 4;
            SaveChanges();
        }
        _isLoaded = true;
    }

    private void SaveChanges()
    {
        var json = JsonSerializer.Serialize(_vocabularies, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_dataPath, json);
    }

    public Task<List<VocabularyItem>> GetAllVocabularyAsync()
    {
        return Task.FromResult(_vocabularies);
    }

    public Task<VocabularyItem?> GetVocabularyByIdAsync(int id)
    {
        var vocab = _vocabularies.FirstOrDefault(v => v.Id == id);
        return Task.FromResult(vocab);
    }

    public Task<VocabularyItem> AddVocabularyAsync(VocabularyItem vocabulary)
    {
        vocabulary.Id = _nextId++;
        vocabulary.CreatedAt = DateTime.Now;
        vocabulary.UpdatedAt = DateTime.Now;
        _vocabularies.Add(vocabulary);
        SaveChanges();
        return Task.FromResult(vocabulary);
    }

    public Task<VocabularyItem?> UpdateVocabularyAsync(int id, VocabularyItem vocabulary)
    {
        var existingVocab = _vocabularies.FirstOrDefault(v => v.Id == id);
        if (existingVocab == null)
            return Task.FromResult<VocabularyItem?>(null);

        existingVocab.EnglishWord = vocabulary.EnglishWord;
        existingVocab.VietnameseMeaning = vocabulary.VietnameseMeaning;
        existingVocab.Example = vocabulary.Example;
        existingVocab.WordType = vocabulary.WordType;
        existingVocab.UpdatedAt = DateTime.Now;

        SaveChanges();
        return Task.FromResult<VocabularyItem?>(existingVocab);
    }

    public Task<bool> DeleteVocabularyAsync(int id)
    {
        var vocab = _vocabularies.FirstOrDefault(v => v.Id == id);
        if (vocab == null)
            return Task.FromResult(false);

        _vocabularies.Remove(vocab);
        SaveChanges();
        return Task.FromResult(true);
    }

}
