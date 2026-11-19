using System.Text.Json.Serialization;

namespace SchoolLibrary.Models;

public enum ReaderRole { Student, Teacher }

public enum BookCondition { Good, Damaged, Lost }

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public string Isbn { get; set; } = "";
    public int MinGrade { get; set; } = 1;
    public int MaxGrade { get; set; } = 12;
    public string Description { get; set; } = "";
    public string Shelf { get; set; } = "";
    public int TotalCopies { get; set; } = 1;

    /// <summary>BR-04: справочни издания и книги „само за четене в библиотеката“.</summary>
    public bool ReferenceOnly { get; set; }

    /// <summary>FR-02: книгата се деактивира, а не се трие, за да остане историята на заеманията.</summary>
    public bool IsActive { get; set; } = true;

    [JsonIgnore]
    public string AgeText => MinGrade <= 1 && MaxGrade >= 12 ? "всички класове" : $"{MinGrade}–{MaxGrade} клас";
}

public class Reader
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public ReaderRole Role { get; set; } = ReaderRole.Student;

    [JsonIgnore]
    public string DisplayName => Role == ReaderRole.Teacher
        ? $"{Id} – {Name} (учител)"
        : $"{Id} – {Name} ({ClassName})";

    /// <summary>Номерът на класа („7Б“ → 7). За учители е null.</summary>
    [JsonIgnore]
    public int? Grade
    {
        get
        {
            var digits = new string(ClassName.TakeWhile(char.IsDigit).ToArray());
            return int.TryParse(digits, out var grade) ? grade : null;
        }
    }
}

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public DateTime LoanedOn { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedOn { get; set; }
    public BookCondition? Condition { get; set; }

    [JsonIgnore]
    public bool IsActive => ReturnedOn == null;
}

/// <summary>NFR-09: правилата са настройки, а не са вградени в логиката.</summary>
public class LibrarySettings
{
    public int LoanDays { get; set; } = 14;
    public int StudentLimit { get; set; } = 3;
    public int TeacherLimit { get; set; } = 5;
    public int ReviewAfterDays { get; set; } = 30;
}

public class LibraryData
{
    public List<Book> Books { get; set; } = new();
    public List<Reader> Readers { get; set; } = new();
    public List<Loan> Loans { get; set; } = new();
    public LibrarySettings Settings { get; set; } = new();
}
