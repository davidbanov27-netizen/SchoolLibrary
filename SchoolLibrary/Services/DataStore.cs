using System.IO;
using System.Text.Json;
using SchoolLibrary.Models;

namespace SchoolLibrary.Services;

/// <summary>
/// Записва данните в JSON файл в %LocalAppData%\SchoolLibrary\library.json.
/// Така проектът няма външни NuGet пакети. По-късно може да се замени със SQLite
/// само чрез нов клас с тези два метода.
/// </summary>
public class DataStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public DataStore(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SchoolLibrary",
            "library.json");
    }

    public string FilePath => _path;

    /// <summary>Съобщение за последната грешка при запис (null, ако всичко е наред).</summary>
    public string? LastError { get; private set; }

    public LibraryData Load()
    {
        try
        {
            if (File.Exists(_path))
            {
                var data = JsonSerializer.Deserialize<LibraryData>(File.ReadAllText(_path), Options);
                if (data != null && data.Books.Count > 0)
                {
                    return data;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            LastError = "Файлът с данни не може да се прочете. Заредени са примерни данни. " + ex.Message;
        }

        return SeedData.Create(DateTime.Today);
    }

    public void Save(LibraryData data)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(data, Options));
            LastError = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LastError = "Данните не са записани: " + ex.Message;
        }
    }
}
