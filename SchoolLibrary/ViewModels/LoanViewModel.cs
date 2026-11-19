using System.Windows.Input;
using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

/// <summary>UC-01 „Заемане“. Всички проверки идват от LibraryService.CheckLoan.</summary>
public class LoanViewModel : PageViewModel
{
    private readonly LibraryService _s;
    private readonly INavigator _nav;

    private Reader? _reader;
    private BookRow? _book;
    private IReadOnlyList<BookRow> _books = Array.Empty<BookRow>();
    private IReadOnlyList<string> _problems = Array.Empty<string>();

    public LoanViewModel(LibraryService s, INavigator nav)
    {
        _s = s;
        _nav = nav;
        Readers = s.Readers;
        LoadBooks();

        ConfirmCommand = new RelayCommand(Confirm, () => CanConfirm);
        CancelCommand = new RelayCommand(() => { ClearMessage(); Reset(); });
        GoReturnCommand = new RelayCommand(() => _nav.Navigate("return"));
    }

    public IReadOnlyList<Reader> Readers { get; }

    public IReadOnlyList<BookRow> Books
    {
        get => _books;
        private set => Set(ref _books, value);
    }

    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand GoReturnCommand { get; }

    public Reader? SelectedReader
    {
        get => _reader;
        set
        {
            if (Set(ref _reader, value))
            {
                ClearMessage();
                Recompute();
            }
        }
    }

    public BookRow? SelectedBook
    {
        get => _book;
        set
        {
            if (Set(ref _book, value))
            {
                ClearMessage();
                Recompute();
            }
        }
    }

    public string ReaderInfo
    {
        get
        {
            if (_reader == null)
            {
                return "";
            }

            var loans = _s.LoansOf(_reader);
            var overdue = loans.Any(_s.IsOverdue) ? "има просрочена книга" : "няма просрочени ✔";
            return $"{_reader.Name} · {_reader.ClassName} · заети: {loans.Count} от {_s.LimitOf(_reader)} · {overdue}";
        }
    }

    public string BookInfo => _book == null ? "" : $"„{_book.Title}“ · налични: {_book.Available} от {_book.Total}";

    public IReadOnlyList<string> Problems => _problems;
    public bool HasReader => _reader != null;
    public bool HasBook => _book != null;
    public bool HasProblems => _problems.Count > 0;
    public bool CanConfirm => _reader != null && _book != null && _problems.Count == 0;
    public string DueText => $"Всички проверки са преминати. Срок за връщане: {_s.Today.AddDays(_s.Settings.LoanDays):dd.MM.yyyy}";

    private void LoadBooks() =>
        Books = _s.Books.Select(b => RowFactory.ToBookRow(_s, b)).ToList();

    private void Recompute()
    {
        _problems = _reader != null && _book != null ? _s.CheckLoan(_reader, _book.Book) : Array.Empty<string>();
        OnPropertyChanged(nameof(ReaderInfo));
        OnPropertyChanged(nameof(BookInfo));
        OnPropertyChanged(nameof(Problems));
        OnPropertyChanged(nameof(HasReader));
        OnPropertyChanged(nameof(HasBook));
        OnPropertyChanged(nameof(HasProblems));
        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(DueText));
    }

    private void Reset()
    {
        _reader = null;
        _book = null;
        LoadBooks();
        OnPropertyChanged(nameof(SelectedReader));
        OnPropertyChanged(nameof(SelectedBook));
        Recompute();
    }

    private void Confirm()
    {
        if (_reader == null || _book == null)
        {
            return;
        }

        var reader = _reader;
        var title = _book.Title;

        try
        {
            var loan = _s.CreateLoan(reader, _book.Book);
            Reset();
            Show(MessageKind.Success, $"Заемането е записано: „{title}“ → {reader.Name}. Срок: {loan.DueDate:dd.MM.yyyy}.");
        }
        catch (InvalidOperationException ex)
        {
            Show(MessageKind.Error, ex.Message);
        }
    }
}
