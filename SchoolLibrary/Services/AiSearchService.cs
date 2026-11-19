using System.Text.RegularExpressions;
using SchoolLibrary.Models;

namespace SchoolLibrary.Services;

public sealed record AiSuggestion(Book Book, string Why, int Score);

/// <summary>
/// Договор за ИИ търсенето. Екраните зависят само от този интерфейс, затова
/// локалната имплементация по-долу може да се замени с истински ИИ модел (HTTP API)
/// без промяна в ViewModel-ите и във View-тата.
///
/// Правила за всяка имплементация (BR-16, BR-17, FR-22, NFR-05):
///  - връща само книги от подадения каталог – никога измислени заглавия;
///  - не получава имена или контакти на ученици;
///  - само предлага; решенията за заемане се взимат от бизнес правилата.
/// </summary>
public interface IAiSearchService
{
    Task<IReadOnlyList<AiSuggestion>> SearchAsync(
        string query, IReadOnlyList<Book> catalog, CancellationToken cancellationToken = default);
}

/// <summary>
/// Локален заместител без интернет: разпознава жанр, клас и ключови думи в заявката.
/// Работи като резервен вариант и показва точно какъв резултат очаква интерфейсът.
/// </summary>
public sealed class LocalAiSearchService : IAiSearchService
{
    private static readonly (string Pattern, string Genre)[] GenreHints =
    {
        ("приключ", "Приключения"),
        ("фентъзи|магия|магьосн|вълшеб", "Фентъзи"),
        ("приказк|принц", "Приказка"),
        ("роман|история|българск", "Роман"),
    };

    public Task<IReadOnlyList<AiSuggestion>> SearchAsync(
        string query, IReadOnlyList<Book> catalog, CancellationToken cancellationToken = default)
    {
        return Task.Run<IReadOnlyList<AiSuggestion>>(() => Score(query, catalog), cancellationToken);
    }

    private static List<AiSuggestion> Score(string query, IReadOnlyList<Book> catalog)
    {
        var q = query.ToLowerInvariant();
        var words = Regex.Split(q, @"\W+").Where(w => w.Length > 3).ToList();

        int? grade = null;
        var gradeMatch = Regex.Match(q, @"(\d+)\s*\.?\s*клас");
        if (gradeMatch.Success && int.TryParse(gradeMatch.Groups[1].Value, out var parsed))
        {
            grade = parsed;
        }

        var results = new List<AiSuggestion>();
        foreach (var book in catalog.Where(b => !b.ReferenceOnly))
        {
            var score = 0;
            var why = new List<string>();

            foreach (var (pattern, genre) in GenreHints)
            {
                if (Regex.IsMatch(q, pattern) && book.Genre == genre)
                {
                    score += 2;
                    why.Add($"жанр „{genre}“");
                }
            }

            if (grade is int g && g >= book.MinGrade && g <= book.MaxGrade)
            {
                score += 1;
                why.Add($"подходяща за {g}. клас");
            }

            var text = $"{book.Title} {book.Description}".ToLowerInvariant();
            score += words.Count(w => text.Contains(w));

            if (score > 0)
            {
                results.Add(new AiSuggestion(book, why.Count > 0 ? string.Join(", ", why) : "съвпада със заявката", score));
            }
        }

        return results.OrderByDescending(r => r.Score).ThenBy(r => r.Book.Title).ToList();
    }
}
