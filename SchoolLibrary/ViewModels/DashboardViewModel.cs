using System.Windows.Input;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public class DashboardViewModel : PageViewModel
{
    public DashboardViewModel(LibraryService s, INavigator nav)
    {
        TotalCopies = s.Books.Sum(b => b.TotalCopies);
        ActiveLoans = s.ActiveLoans.Count();

        var reminders = new List<string>();
        foreach (var loan in s.ActiveLoans.OrderBy(l => l.DueDate))
        {
            var row = RowFactory.ToLoanRow(s, loan);
            var daysLeft = (loan.DueDate.Date - s.Today).Days;
            if (row.IsOverdue)
            {
                OverdueCount++;
                reminders.Add($"⚠ {row.ReaderName} ({row.ClassName}) – „{row.BookTitle}“, закъснение {row.DaysOverdue} дни");
            }
            else if (daysLeft <= 2)
            {
                reminders.Add($"⏰ {row.ReaderName} ({row.ClassName}) – „{row.BookTitle}“, срок {loan.DueDate:dd.MM.yyyy}");
            }
        }

        Reminders = reminders.Count > 0 ? reminders : new List<string> { "Няма напомняния." };

        LoanCommand = new RelayCommand(() => nav.Navigate("loan"));
        ReturnCommand = new RelayCommand(() => nav.Navigate("return"));
        CatalogCommand = new RelayCommand(() => nav.Navigate("catalog"));
    }

    public int TotalCopies { get; }
    public int ActiveLoans { get; }
    public int OverdueCount { get; private set; }
    public bool HasOverdue => OverdueCount > 0;
    public IReadOnlyList<string> Reminders { get; }

    public ICommand LoanCommand { get; }
    public ICommand ReturnCommand { get; }
    public ICommand CatalogCommand { get; }
}
