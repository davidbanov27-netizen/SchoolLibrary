using System.Net.Http;
using System.Windows.Input;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

/// <summary>Екран „Търсене“ за читател: по думи и с ИИ (RD-01, RD-02).</summary>
public class SearchViewModel : PageViewModel
{
    private readonly LibraryService _s;
    private readonly IAiSearchService _ai;

    private string _query = "";
    private IReadOnlyList<ResultRow> _results = Array.Empty<ResultRow>();
    private bool _isBusy;

    public SearchViewModel(LibraryService s, IAiSearchService ai)
    {
        _s = s;
        _ai = ai;
        KeywordCommand = new RelayCommand(SearchKeyword, () => !IsBusy);
        AskAiCommand = new RelayCommand(() => _ = AskAiAsync(), () => !IsBusy);
    }

    public ICommand KeywordCommand { get; }
    public ICommand AskAiCommand { get; }

    public string Query
    {
        get => _query;
        set => Set(ref _query, value);
    }

    public IReadOnlyList<ResultRow> Results
    {
        get => _results;
        private set
        {
            if (Set(ref _results, value))
            {
                OnPropertyChanged(nameof(HasResults));
            }
        }
    }

    public bool HasResults => _results.Count > 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set => Set(ref _isBusy, value);
    }

    private void SearchKeyword()
    {
        if (Query.Trim().Length == 0)
        {
            Results = Array.Empty<ResultRow>();
            Show(MessageKind.Warning, "Напишете заглавие, автор или тема.");
            return;
        }

        Results = _s.SearchKeyword(Query).Select(b => RowFactory.ToResultRow(_s, b, "")).ToList();
        if (Results.Count == 0)
        {
            Show(MessageKind.Warning,
                "Няма намерени книги. Опитайте да опишете с думи какво търсите и натиснете „Попитай ИИ“.");
        }
        else
        {
            ClearMessage();
        }
    }

    private async Task AskAiAsync()
    {
        if (Query.Trim().Length == 0)
        {
            Results = Array.Empty<ResultRow>();
            Show(MessageKind.Warning, "Опишете с думи какво търсите, напр. „кратка книга за приключения за 6. клас“.");
            return;
        }

        IsBusy = true;
        try
        {
            // Към ИИ отива само текстът на заявката и каталогът – без данни за ученика (NFR-05).
            var suggestions = await _ai.SearchAsync(Query, _s.Books);
            Results = suggestions.Select(x => RowFactory.ToResultRow(_s, x.Book, x.Why)).ToList();

            if (Results.Count == 0)
            {
                Show(MessageKind.Warning, "ИИ не намери подходящи книги в каталога. Опитайте с други думи.");
            }
            else
            {
                Show(MessageKind.Info, "Резултати от ИИ търсене (само от наличния каталог).");
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            Show(MessageKind.Error, "ИИ търсенето не е налично. Ползвайте „Търси по думи“.");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
