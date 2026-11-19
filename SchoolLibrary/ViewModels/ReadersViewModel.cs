using System.Windows.Input;
using SchoolLibrary.Models;
using SchoolLibrary.Services;

namespace SchoolLibrary.ViewModels;

public class ReadersViewModel : PageViewModel
{
    private readonly LibraryService _s;
    private IReadOnlyList<ReaderRow> _readers = Array.Empty<ReaderRow>();
    private string _newName = "";
    private string _newClass = "";
    private bool _newIsTeacher;

    public ReadersViewModel(LibraryService s)
    {
        _s = s;
        SaveCommand = new RelayCommand(Save);
        Refresh();
    }

    public ICommand SaveCommand { get; }

    public IReadOnlyList<ReaderRow> Readers
    {
        get => _readers;
        private set => Set(ref _readers, value);
    }

    public string NewName { get => _newName; set => Set(ref _newName, value); }
    public string NewClass { get => _newClass; set => Set(ref _newClass, value); }

    public bool NewIsTeacher
    {
        get => _newIsTeacher;
        set
        {
            if (Set(ref _newIsTeacher, value))
            {
                OnPropertyChanged(nameof(IsStudent));
            }
        }
    }

    public bool IsStudent => !_newIsTeacher;

    private void Refresh() =>
        Readers = _s.Readers.Select(r => RowFactory.ToReaderRow(_s, r)).ToList();

    private void Save()
    {
        var name = NewName.Trim();
        var cls = NewClass.Trim();

        if (name.Length == 0 || (!NewIsTeacher && cls.Length == 0))
        {
            Show(MessageKind.Error, "Попълнете име и клас.");
            return;
        }

        var reader = _s.AddReader(name, NewIsTeacher ? "Учител" : cls,
            NewIsTeacher ? ReaderRole.Teacher : ReaderRole.Student);

        NewName = "";
        NewClass = "";
        NewIsTeacher = false;
        Refresh();
        Show(MessageKind.Success, $"Читателят е регистриран с номер {reader.Id}.");
    }
}
