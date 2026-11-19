using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public sealed class InterestOption : ViewModelBase
{
    private readonly Action _changed;
    private bool _isSelected;

    public InterestOption(string name, bool isSelected, Action changed)
    {
        Name = name;
        _isSelected = isSelected;
        _changed = changed;
    }

    public string Name { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (Set(ref _isSelected, value))
            {
                _changed();
            }
        }
    }
}

/// <summary>Препоръки по интереси и клас, без вече ползвани книги (RD-03).</summary>
public class RecsViewModel : PageViewModel
{
    private readonly LibraryService _s;
    private readonly Reader _reader;
    private IReadOnlyList<ResultRow> _items = Array.Empty<ResultRow>();

    public RecsViewModel(LibraryService s, Reader reader)
    {
        _s = s;
        _reader = reader;

        var genres = s.Books.Where(b => !b.ReferenceOnly).Select(b => b.Genre).Distinct().OrderBy(g => g).ToList();
        var first = genres.Contains("Приключения") ? "Приключения" : genres.FirstOrDefault();
        Interests = genres.Select(g => new InterestOption(g, g == first, Refresh)).ToList();
        Refresh();
    }

    public IReadOnlyList<InterestOption> Interests { get; }

    public IReadOnlyList<ResultRow> Items
    {
        get => _items;
        private set
        {
            if (Set(ref _items, value))
            {
                OnPropertyChanged(nameof(HasItems));
            }
        }
    }

    public bool HasItems => _items.Count > 0;

    private void Refresh()
    {
        var chosen = Interests.Where(i => i.IsSelected).Select(i => i.Name).ToHashSet();
        if (chosen.Count == 0)
        {
            Items = Array.Empty<ResultRow>();
            Show(MessageKind.Warning, "Изберете поне един интерес.");
            return;
        }

        var alreadyUsed = _s.AllLoans.Where(l => l.ReaderId == _reader.Id).Select(l => l.BookId).ToHashSet();
        var grade = _reader.Grade;

        Items = _s.Books
            .Where(b => !b.ReferenceOnly && chosen.Contains(b.Genre) && !alreadyUsed.Contains(b.Id))
            .Where(b => grade is not int g || (g >= b.MinGrade && g <= b.MaxGrade))
            .Take(5)
            .Select(b => RowFactory.ToResultRow(_s, b, $"съвпада с интерес „{b.Genre}“" + (grade != null ? $" и е подходяща за {grade}. клас" : "")))
            .ToList();

        if (Items.Count == 0)
        {
            Show(MessageKind.Info, "Няма нови препоръки за избраните интереси. Опитайте с други.");
        }
        else
        {
            ClearMessage();
        }
    }
}
