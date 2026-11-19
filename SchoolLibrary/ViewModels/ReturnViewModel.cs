using System.Windows.Input;
using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

/// <summary>UC-02 „Връщане“.</summary>
public class ReturnViewModel : PageViewModel
{
    private readonly LibraryService _s;

    private Reader? _reader;
    private IReadOnlyList<LoanRow> _loans = Array.Empty<LoanRow>();
    private LoanRow? _selectedLoan;
    private ConditionOption _selectedCondition;

    public ReturnViewModel(LibraryService s)
    {
        _s = s;
        Readers = s.Readers;
        Conditions = new[]
        {
            new ConditionOption("Добро", BookCondition.Good),
            new ConditionOption("Повредена", BookCondition.Damaged),
            new ConditionOption("Загубена", BookCondition.Lost),
        };
        _selectedCondition = Conditions[0];

        ConfirmCommand = new RelayCommand(Confirm, () => _selectedLoan != null);
        CancelCommand = new RelayCommand(() =>
        {
            ClearMessage();
            SelectedReader = null;
        });
    }

    public IReadOnlyList<Reader> Readers { get; }
    public IReadOnlyList<ConditionOption> Conditions { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand CancelCommand { get; }

    public Reader? SelectedReader
    {
        get => _reader;
        set
        {
            if (Set(ref _reader, value))
            {
                ClearMessage();
                LoadLoans();
            }
        }
    }

    public IReadOnlyList<LoanRow> ActiveLoans
    {
        get => _loans;
        private set => Set(ref _loans, value);
    }

    public LoanRow? SelectedLoan
    {
        get => _selectedLoan;
        set
        {
            if (Set(ref _selectedLoan, value))
            {
                OnPropertyChanged(nameof(HasSelection));
            }
        }
    }

    public ConditionOption SelectedCondition
    {
        get => _selectedCondition;
        set => Set(ref _selectedCondition, value ?? Conditions[0]);
    }

    public bool HasReader => _reader != null;
    public bool HasLoans => _loans.Count > 0;
    public bool NoLoans => _reader != null && _loans.Count == 0;
    public bool HasSelection => _selectedLoan != null;

    private void LoadLoans()
    {
        ActiveLoans = _reader == null
            ? Array.Empty<LoanRow>()
            : _s.LoansOf(_reader).OrderBy(l => l.DueDate).Select(l => RowFactory.ToLoanRow(_s, l)).ToList();
        SelectedLoan = null;
        SelectedCondition = Conditions[0];
        OnPropertyChanged(nameof(HasReader));
        OnPropertyChanged(nameof(HasLoans));
        OnPropertyChanged(nameof(NoLoans));
    }

    private void Confirm()
    {
        var row = _selectedLoan;
        if (row == null)
        {
            return;
        }

        try
        {
            var result = _s.ReturnLoan(row.Loan, _selectedCondition.Value);

            var text = $"Връщането е записано: „{result.Book.Title}“ ({_selectedCondition.Label}).";
            if (result.LateDays > 0)
            {
                text += $" Закъснение: {result.LateDays} дни.";
            }

            if (result.Condition != BookCondition.Good)
            {
                text += " Копието е изключено от наличността.";
            }

            if (result.BlockLifted)
            {
                text += " Блокировката на читателя е снета.";
            }

            LoadLoans();
            Show(result.LateDays > 0 ? MessageKind.Warning : MessageKind.Success, text);
        }
        catch (InvalidOperationException ex)
        {
            Show(MessageKind.Error, ex.Message);
        }
    }
}
