using SchoolLibrary.Models;

namespace SchoolLibrary.Services;

public sealed record ReturnResult(Book Book, Reader Reader, BookCondition Condition, int LateDays, bool BlockLifted);

/// <summary>
/// Цялата бизнес логика на библиотеката (BR-01 … BR-15 от Етап 2).
/// Екраните не пазят правила – те само извикват този клас.
/// </summary>
public class LibraryService
{
    private static readonly char[] Separators =
        { ' ', '\t', ',', '.', ';', ':', '-', '–', '"', '„', '“', '(', ')', '!', '?' };

    private readonly DataStore _store;
    private readonly LibraryData _data;

    public LibraryService(DataStore store)
    {
        _store = store;
        _data = store.Load();
    }

    public DateTime Today => DateTime.Today;
    public LibrarySettings Settings => _data.Settings;
    public string? LastSaveError => _store.LastError;

    // ===== Четене =====

    public IReadOnlyList<Book> Books => _data.Books.Where(b => b.IsActive).OrderBy(b => b.Title).ToList();
    public IReadOnlyList<Reader> Readers => _data.Readers.OrderBy(r => r.Id).ToList();
    public IReadOnlyList<Loan> AllLoans => _data.Loans;
    public IEnumerable<Loan> ActiveLoans => _data.Loans.Where(l => l.IsActive);

    public Book? GetBook(int id) => _data.Books.FirstOrDefault(b => b.Id == id);
    public Reader? GetReader(int id) => _data.Readers.FirstOrDefault(r => r.Id == id);

    public int Available(Book book) => book.TotalCopies - ActiveLoans.Count(l => l.BookId == book.Id);
    public IReadOnlyList<Loan> LoansOf(Reader reader) => ActiveLoans.Where(l => l.ReaderId == reader.Id).ToList();

    public bool IsOverdue(Loan loan) => loan.IsActive && loan.DueDate.Date < Today;
    public int DaysOverdue(Loan loan) => IsOverdue(loan) ? (Today - loan.DueDate.Date).Days : 0;

    public int LimitOf(Reader reader) =>
        reader.Role == ReaderRole.Teacher ? Settings.TeacherLimit : Settings.StudentLimit;

    // ===== Заемане (BR-01 … BR-06) =====

    /// <summary>Връща причините, поради които заемането не е позволено. Празен списък = позволено.</summary>
    public IReadOnlyList<string> CheckLoan(Reader reader, Book book)
    {
        var problems = new List<string>();

        if (book.ReferenceOnly)
        {
            problems.Add("Книгата е само за четене в библиотеката и не може да се изнася.");
        }

        if (Available(book) < 1)
        {
            problems.Add("Няма налично копие.");
        }

        var loans = LoansOf(reader);
        var limit = LimitOf(reader);
        if (loans.Count >= limit)
        {
            problems.Add($"Достигнат лимит от {limit} книги.");
        }

        var overdue = loans.FirstOrDefault(IsOverdue);
        if (overdue != null)
        {
            problems.Add($"Читателят има просрочена книга: „{GetBook(overdue.BookId)?.Title}“ " +
                         $"(срок {overdue.DueDate:dd.MM.yyyy}). Върнете я, за да заемете нова.");
        }

        if (loans.Any(l => l.BookId == book.Id))
        {
            problems.Add("Читателят вече има това заглавие.");
        }

        return problems;
    }

    public Loan CreateLoan(Reader reader, Book book)
    {
        var problems = CheckLoan(reader, book);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", problems));
        }

        var loan = new Loan
        {
            Id = NextId(_data.Loans.Select(l => l.Id)),
            BookId = book.Id,
            ReaderId = reader.Id,
            LoanedOn = Today,
            DueDate = Today.AddDays(Settings.LoanDays),
        };
        _data.Loans.Add(loan);
        Save();
        return loan;
    }

    // ===== Връщане (BR-07, BR-09, BR-11, BR-13, BR-15) =====

    public ReturnResult ReturnLoan(Loan loan, BookCondition condition)
    {
        if (!loan.IsActive)
        {
            throw new InvalidOperationException("Заемането вече е затворено.");
        }

        var book = GetBook(loan.BookId) ?? throw new InvalidOperationException("Книгата не е намерена.");
        var reader = GetReader(loan.ReaderId) ?? throw new InvalidOperationException("Читателят не е намерен.");

        var late = DaysOverdue(loan); // преди да затворим заемането

        loan.ReturnedOn = Today;
        loan.Condition = condition;

        // Повредено или загубено копие излиза от наличността (решението за обезщетение е извън системата).
        if (condition != BookCondition.Good && book.TotalCopies > 0)
        {
            book.TotalCopies--;
        }

        var lifted = late > 0 && !LoansOf(reader).Any(IsOverdue);
        Save();
        return new ReturnResult(book, reader, condition, late, lifted);
    }

    // ===== Каталог и читатели =====

    public Book? FindDuplicate(string isbn, string title, string author) =>
        _data.Books.FirstOrDefault(b =>
            (isbn.Length > 0 && b.Isbn == isbn) ||
            (b.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
             b.Author.Equals(author, StringComparison.OrdinalIgnoreCase)));

    public Book AddBook(Book book)
    {
        book.Id = NextId(_data.Books.Select(b => b.Id));
        _data.Books.Add(book);
        Save();
        return book;
    }

    /// <summary>Деактивира книга. Не е позволено, докато има неврънати копия.</summary>
    public bool TryDeactivateBook(Book book, out string reason)
    {
        if (ActiveLoans.Any(l => l.BookId == book.Id))
        {
            reason = "Книгата има неврънати копия. Първо ги върнете.";
            return false;
        }

        book.IsActive = false;
        Save();
        reason = "";
        return true;
    }

    public Reader AddReader(string name, string className, ReaderRole role)
    {
        var reader = new Reader
        {
            Id = _data.Readers.Count == 0 ? 101 : _data.Readers.Max(r => r.Id) + 1,
            Name = name,
            ClassName = className,
            Role = role,
        };
        _data.Readers.Add(reader);
        Save();
        return reader;
    }

    // ===== Търсене по думи (FR-18) =====

    /// <summary>Търсене по заглавие, автор, жанр и описание с толерантност към една правописна грешка.</summary>
    public IReadOnlyList<Book> SearchKeyword(string query)
    {
        var terms = Tokenize(query);
        if (terms.Count == 0)
        {
            return Books;
        }

        return Books.Where(book =>
        {
            var words = Tokenize($"{book.Title} {book.Author} {book.Genre} {book.Description}");
            return terms.All(term => words.Any(word => Matches(word, term)));
        }).ToList();
    }

    private static bool Matches(string word, string term) =>
        word.Contains(term) || (term.Length >= 4 && Levenshtein(word, term) <= 1);

    private static List<string> Tokenize(string text) =>
        text.ToLowerInvariant().Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToList();

    /// <summary>Разстояние на Левенщайн: колко единични промени делят две думи.</summary>
    public static int Levenshtein(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];
        for (var i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (var j = 0; j <= b.Length; j++) d[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[a.Length, b.Length];
    }

    // ===== Помощни =====

    private static int NextId(IEnumerable<int> ids) => ids.DefaultIfEmpty(0).Max() + 1;

    private void Save() => _store.Save(_data);
}
