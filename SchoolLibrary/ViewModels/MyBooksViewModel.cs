using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

/// <summary>RD-05: читателят вижда само своите заемания (FR-08).</summary>
public class MyBooksViewModel : PageViewModel
{
    public MyBooksViewModel(LibraryService s, Reader reader)
    {
        Rows = s.LoansOf(reader).OrderBy(l => l.DueDate).Select(l => RowFactory.ToLoanRow(s, l)).ToList();
    }

    public IReadOnlyList<LoanRow> Rows { get; }
    public bool HasRows => Rows.Count > 0;
    public bool IsEmpty => Rows.Count == 0;
}
