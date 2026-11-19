using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public class OverdueViewModel : PageViewModel
{
    public OverdueViewModel(LibraryService s)
    {
        Rows = s.ActiveLoans
            .Where(s.IsOverdue)
            .OrderBy(l => l.DueDate)
            .Select(l => RowFactory.ToLoanRow(s, l))
            .ToList();
    }

    public IReadOnlyList<LoanRow> Rows { get; }
    public bool HasRows => Rows.Count > 0;
    public bool IsEmpty => Rows.Count == 0;
}
