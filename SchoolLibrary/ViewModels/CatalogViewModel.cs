using System.Windows.Input;
using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public class CatalogViewModel : PageViewModel
{
    private readonly LibraryService _s;

    private string _searchText = "";
    private IReadOnlyList<BookRow> _books = Array.Empty<BookRow>();
    private BookRow? _selectedBook;
    private bool _isAdding;

    private string _newTitle = "";
    private string _newAuthor = "";
    private string _newGenre = "";
    private string _newIsbn = "";
    private string _newShelf = "";
    private string _newDescription = "";
    private string _newCopies = "1";
    private string _newMinGrade = "1";
    private string _newMaxGrade = "12";
    private bool _newReferenceOnly;

    public CatalogViewModel(LibraryService s)
    {
        _s = s;
        AddCommand = new RelayCommand(() => { ClearMessage(); IsAdding = true; });
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => { ResetForm(); IsAdding = false; });
        DeactivateCommand = new RelayCommand(Deactivate, () => SelectedBook != null);
        Refresh();
    }

    public ICommand AddCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeactivateCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (Set(ref _searchText, value))
            {
                Refresh();
            }
        }
    }

    public IReadOnlyList<BookRow> Books
    {
        get => _books;
        private set => Set(ref _books, value);
    }

    public BookRow? SelectedBook
    {
        get => _selectedBook;
        set
        {
            if (Set(ref _selectedBook, value))
            {
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public bool HasSelection => _selectedBook != null;

    public bool IsAdding
    {
        get => _isAdding;
        private set => Set(ref _isAdding, value);
    }

    public string NewTitle { get => _newTitle; set => Set(ref _newTitle, value); }
    public string NewAuthor { get => _newAuthor; set => Set(ref _newAuthor, value); }
    public string NewGenre { get => _newGenre; set => Set(ref _newGenre, value); }
    public string NewIsbn { get => _newIsbn; set => Set(ref _newIsbn, value); }
    public string NewShelf { get => _newShelf; set => Set(ref _newShelf, value); }
    public string NewDescription { get => _newDescription; set => Set(ref _newDescription, value); }
    public string NewCopies { get => _newCopies; set => Set(ref _newCopies, value); }
    public string NewMinGrade { get => _newMinGrade; set => Set(ref _newMinGrade, value); }
    public string NewMaxGrade { get => _newMaxGrade; set => Set(ref _newMaxGrade, value); }
    public bool NewReferenceOnly { get => _newReferenceOnly; set => Set(ref _newReferenceOnly, value); }

    private void Refresh()
    {
        SelectedBook = null;
        Books = _s.SearchKeyword(_searchText).Select(b => RowFactory.ToBookRow(_s, b)).ToList();
    }

    private void Save()
    {
        var title = NewTitle.Trim();
        var author = NewAuthor.Trim();
        var isbn = NewIsbn.Trim();

        if (title.Length == 0 || author.Length == 0)
        {
            Show(MessageKind.Error, "Попълнете заглавие и автор.");
            return;
        }

        if (!int.TryParse(NewCopies, out var copies) || copies < 1)
        {
            Show(MessageKind.Error, "Броят копия трябва да е число, не по-малко от 1.");
            return;
        }

        if (!int.TryParse(NewMinGrade, out var min) || !int.TryParse(NewMaxGrade, out var max)
            || min < 1 || max > 12 || min > max)
        {
            Show(MessageKind.Error, "Класовете трябва да са числа от 1 до 12, като първият не е по-голям от втория.");
            return;
        }

        var duplicate = _s.FindDuplicate(isbn, title, author);
        if (duplicate != null)
        {
            Show(MessageKind.Error, $"Вече има такава книга: „{duplicate.Title}“. Не добавяйте дубликат.");
            return;
        }

        _s.AddBook(new Book
        {
            Title = title,
            Author = author,
            Genre = NewGenre.Trim().Length > 0 ? NewGenre.Trim() : "—",
            Isbn = isbn,
            Shelf = NewShelf.Trim(),
            Description = NewDescription.Trim(),
            TotalCopies = copies,
            MinGrade = min,
            MaxGrade = max,
            ReferenceOnly = NewReferenceOnly,
        });

        ResetForm();
        IsAdding = false;
        Refresh();
        Show(MessageKind.Success, $"Книгата „{title}“ е добавена.");
    }

    private void Deactivate()
    {
        var row = SelectedBook;
        if (row == null)
        {
            return;
        }

        if (_s.TryDeactivateBook(row.Book, out var reason))
        {
            Refresh();
            Show(MessageKind.Success, $"Книгата „{row.Title}“ е премахната от каталога.");
        }
        else
        {
            Show(MessageKind.Error, reason);
        }
    }

    private void ResetForm()
    {
        NewTitle = "";
        NewAuthor = "";
        NewGenre = "";
        NewIsbn = "";
        NewShelf = "";
        NewDescription = "";
        NewCopies = "1";
        NewMinGrade = "1";
        NewMaxGrade = "12";
        NewReferenceOnly = false;
    }
}
