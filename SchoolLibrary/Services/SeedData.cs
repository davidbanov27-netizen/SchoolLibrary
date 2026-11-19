using SchoolLibrary.Models;

namespace SchoolLibrary.Services;

/// <summary>Примерни данни за първо стартиране. Датите са спрямо днешния ден.</summary>
public static class SeedData
{
    public static LibraryData Create(DateTime today)
    {
        var data = new LibraryData();

        data.Books.AddRange(new[]
        {
            new Book { Id = 1, Title = "Под игото", Author = "Иван Вазов", Genre = "Роман", MinGrade = 8, MaxGrade = 12, TotalCopies = 3, Shelf = "А-12", Description = "Роман за Априлското въстание." },
            new Book { Id = 2, Title = "Хари Потър и философския камък", Author = "Дж. К. Роулинг", Genre = "Фентъзи", MinGrade = 5, MaxGrade = 8, TotalCopies = 2, Shelf = "Б-03", Description = "Момче открива, че е магьосник." },
            new Book { Id = 3, Title = "Островът на съкровищата", Author = "Р. Л. Стивънсън", Genre = "Приключения", MinGrade = 5, MaxGrade = 7, TotalCopies = 2, Shelf = "Б-07", Description = "Пирати, карта и съкровище." },
            new Book { Id = 4, Title = "Тримата мускетари", Author = "Александър Дюма", Genre = "Приключения", MinGrade = 6, MaxGrade = 9, TotalCopies = 1, Shelf = "Б-09", Description = "Приключения във Франция през 17. век." },
            new Book { Id = 5, Title = "Малкият принц", Author = "Антоан дьо Сент-Екзюпери", Genre = "Приказка", MinGrade = 3, MaxGrade = 7, TotalCopies = 2, Shelf = "В-01", Description = "Философска приказка за дружбата." },
            new Book { Id = 6, Title = "Български тълковен речник", Author = "Колектив", Genre = "Справочник", MinGrade = 1, MaxGrade = 12, TotalCopies = 1, Shelf = "Р-01", Description = "Само за четене в библиотеката.", ReferenceOnly = true },
            new Book { Id = 7, Title = "Хобитът", Author = "Дж. Р. Р. Толкин", Genre = "Фентъзи", MinGrade = 6, MaxGrade = 9, TotalCopies = 2, Shelf = "Б-04", Description = "Билбо тръгва на неочаквано пътешествие." },
            new Book { Id = 8, Title = "Пипи Дългото чорапче", Author = "Астрид Линдгрен", Genre = "Приказка", MinGrade = 2, MaxGrade = 5, TotalCopies = 2, Shelf = "В-02", Description = "Най-силното момиче на света." },
            new Book { Id = 9, Title = "Бай Ганьо", Author = "Алеко Константинов", Genre = "Роман", MinGrade = 9, MaxGrade = 12, TotalCopies = 2, Shelf = "А-14", Description = "Сатира за българина и Европа." },
        });

        data.Readers.AddRange(new[]
        {
            new Reader { Id = 101, Name = "Иван Петров", ClassName = "7Б", Role = ReaderRole.Student },
            new Reader { Id = 102, Name = "Мария Георгиева", ClassName = "6А", Role = ReaderRole.Student },
            new Reader { Id = 103, Name = "Георги Димитров", ClassName = "8В", Role = ReaderRole.Student },
            new Reader { Id = 104, Name = "Елена Стоянова", ClassName = "Учител", Role = ReaderRole.Teacher },
        });

        // Активни заемания: dueOffset е колко дни от днес е срокът (отрицателно = просрочено).
        Loan Active(int id, int book, int reader, int dueOffset) => new()
        {
            Id = id, BookId = book, ReaderId = reader,
            LoanedOn = today.AddDays(dueOffset - 14),
            DueDate = today.AddDays(dueOffset),
        };

        // Приключени заемания за статистиката и за препоръките.
        Loan Done(int id, int book, int reader, int loanedDaysAgo, int keptDays) => new()
        {
            Id = id, BookId = book, ReaderId = reader,
            LoanedOn = today.AddDays(-loanedDaysAgo),
            DueDate = today.AddDays(-loanedDaysAgo + 14),
            ReturnedOn = today.AddDays(-loanedDaysAgo + keptDays),
            Condition = BookCondition.Good,
        };

        data.Loans.AddRange(new[]
        {
            Active(1, 1, 101, 8),
            Active(2, 2, 101, -7),
            Active(3, 3, 102, 5),
            Active(4, 5, 102, 3),
            Active(5, 4, 102, 9),
            Active(6, 2, 103, -20),

            Done(11, 1, 103, 75, 12),
            Done(12, 3, 101, 52, 10),
            Done(13, 5, 104, 38, 7),
            Done(14, 1, 102, 95, 13),
            Done(15, 4, 103, 33, 14),
            Done(16, 7, 101, 20, 9),
            Done(17, 2, 102, 48, 13),
            Done(18, 3, 103, 22, 5),
        });

        return data;
    }
}
