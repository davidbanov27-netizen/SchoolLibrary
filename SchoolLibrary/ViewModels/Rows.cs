using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

// Редовете са прости „само за показване“ обекти, подготвени за DataGrid-овете.

public sealed record BookRow(Book Book, int Available)
{
    public int Id => Book.Id;
    public string Title => Book.Title;
    public string Author => Book.Author;
    public string Genre => Book.Genre;
    public string AgeText => Book.AgeText;
    public string Shelf => Book.Shelf;
    public string Description => Book.Description;
    public int Total => Book.TotalCopies;
    public bool ReferenceOnly => Book.ReferenceOnly;
    public string Availability => $"{Available} / {Total}";
    public bool IsAvailable => Available > 0;
    public string Label => $"{Title} (налични {Available})";
}

public sealed record LoanRow(Loan Loan, string BookTitle, string ReaderName, string ClassName, int DaysOverdue, bool NeedsReview)
{
    public DateTime DueDate => Loan.DueDate;
    public bool IsOverdue => DaysOverdue > 0;

    public string StatusText =>
        !IsOverdue ? "В срок"
        : NeedsReview ? $"Просрочена ({DaysOverdue} дни) – за разглеждане"
        : $"Просрочена ({DaysOverdue} дни)";
}

public sealed record ReaderRow(Reader Reader, int LoanCount, int Limit, bool HasOverdue)
{
    public int Id => Reader.Id;
    public string Name => Reader.Name;
    public string ClassName => Reader.ClassName;
    public string LoansText => $"{LoanCount} / {Limit}";
    public string StatusText => HasOverdue ? "Има просрочена" : "В ред";
}

public sealed record BarRow(string Label, int Value, double BarWidth);

public sealed record ConditionOption(string Label, BookCondition Value);

public sealed record ResultRow(Book Book, string Why, bool IsAvailable)
{
    public string Title => Book.Title;
    public string Subtitle => $"{Book.Author} · {Book.AgeText}";
    public string Shelf => $"Рафт {Book.Shelf}";
    public bool HasWhy => Why.Length > 0;
    public string WhyText => $"Защо: {Why}";
    public string AvailabilityText => IsAvailable ? "налична ✔" : "няма налично ✖";
}

public static class RowFactory
{
    public static BookRow ToBookRow(LibraryService s, Book b) => new(b, s.Available(b));

    public static LoanRow ToLoanRow(LibraryService s, Loan l)
    {
        var book = s.GetBook(l.BookId);
        var reader = s.GetReader(l.ReaderId);
        var days = s.DaysOverdue(l);
        return new LoanRow(l, book?.Title ?? "?", reader?.Name ?? "?", reader?.ClassName ?? "", days,
            days > s.Settings.ReviewAfterDays);
    }

    public static ReaderRow ToReaderRow(LibraryService s, Reader r)
    {
        var loans = s.LoansOf(r);
        return new ReaderRow(r, loans.Count, s.LimitOf(r), loans.Any(s.IsOverdue));
    }

    public static ResultRow ToResultRow(LibraryService s, Book b, string why) =>
        new(b, why, s.Available(b) > 0);
}
