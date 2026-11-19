using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public class StatsViewModel : PageViewModel
{
    private const double MaxBar = 220.0;

    public StatsViewModel(LibraryService s)
    {
        var all = s.AllLoans;

        var top = all
            .GroupBy(l => l.BookId)
            .Select(g => new { Book = s.GetBook(g.Key), Count = g.Count() })
            .Where(x => x.Book != null)
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();
        var topMax = top.Count > 0 ? top.Max(x => x.Count) : 1;
        TopBooks = top.Select(x => new BarRow(x.Book!.Title, x.Count, x.Count * MaxBar / topMax)).ToList();

        var firstOfMonth = new DateTime(s.Today.Year, s.Today.Month, 1);
        var months = Enumerable.Range(0, 4).Select(i => firstOfMonth.AddMonths(i - 3)).ToList();
        var counts = months
            .Select(m => (Label: m.ToString("MM.yyyy"), Count: all.Count(l => l.LoanedOn.Year == m.Year && l.LoanedOn.Month == m.Month)))
            .ToList();
        var monthMax = Math.Max(1, counts.Max(c => c.Count));
        ByMonth = counts.Select(c => new BarRow(c.Label, c.Count, c.Count * MaxBar / monthMax)).ToList();

        OverdueNow = s.ActiveLoans.Count(s.IsOverdue);
    }

    public IReadOnlyList<BarRow> TopBooks { get; }
    public IReadOnlyList<BarRow> ByMonth { get; }
    public int OverdueNow { get; }
}
