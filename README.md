# Училищна библиотека – WPF (.NET 10)

Десктоп приложение за Windows: каталог, читатели, заемане и връщане, просрочени книги,
статистика и търсене/препоръки с ИИ. Реализира Етапи 1–3 от проекта.

## Как се отваря

1. Нужен е **Visual Studio 2026** (или 2022 с .NET 10 SDK) с workload **„.NET desktop development“**.
2. Отворете `SchoolLibrary.sln`.
3. Натиснете **F5**. Няма външни NuGet пакети.

Целева платформа: `net10.0-windows` (виж `SchoolLibrary/SchoolLibrary.csproj`).

> **Бележка:** кодът е написан без компилатор под ръка. Ако Visual Studio покаже
> грешка или предупреждение при първото компилиране, изпратете текста ѝ и ще я поправим.

## Структура

```
SchoolLibrary.sln
SchoolLibrary/
├── SchoolLibrary.csproj
├── App.xaml / App.xaml.cs        старт на приложението, свързва услугите
├── MainWindow.xaml               меню вляво + текущ екран; DataTemplate: ViewModel → View
├── Themes/Styles.xaml            цветове, бутони, таблици, карти
├── Models/Models.cs              Book, Reader, Loan, LibrarySettings, LibraryData
├── Services/
│   ├── LibraryService.cs         ВСИЧКИ бизнес правила (BR-01…BR-15)
│   ├── AiSearchService.cs        интерфейс IAiSearchService + локален заместител
│   ├── DataStore.cs              запис/четене в JSON
│   └── SeedData.cs               примерни данни за първо стартиране
├── ViewModels/                   логиката на екраните (MVVM)
│   ├── MainViewModel.cs          вход, меню, навигация
│   ├── Infrastructure.cs         ViewModelBase, RelayCommand, PageViewModel
│   ├── Rows.cs                   редове за таблиците
│   └── Dashboard / Catalog / Readers / Loan / Return / Overdue / Stats /
│       Search / Recs / MyBooks ViewModel
└── Views/                        XAML екраните (по един UserControl на екран)
```

**Принцип:** View-тата само показват и свързват (binding), ViewModel-ите държат състоянието
на екрана, а правилата са само в `LibraryService`. Така правилата се тестват без интерфейс.

## Какво работи

| От прототипа / Етап 3 | Къде е |
|---|---|
| Вход като библиотекар или читател | `LoginView`, `LoginViewModel` |
| Табло и напомняния | `DashboardView` |
| Каталог: търсене, нова книга, премахване, подробности | `CatalogView` |
| Читатели: списък, регистрация | `ReadersView` |
| **Заемане (UC-01)** с всички отказни причини | `LoanView` → `LibraryService.CheckLoan` |
| **Връщане (UC-02)**: в срок, късно, повредена, загубена | `ReturnView` → `LibraryService.ReturnLoan` |
| Просрочени книги | `OverdueView` |
| Статистика | `StatsView` |
| Търсене по думи (с толерантност към 1 грешка) и с ИИ | `SearchView` |
| Препоръки по интереси и клас, без вече ползвани книги | `RecsView` |
| Моите книги | `MyBooksView` |

Данните се пазят в `%LocalAppData%\SchoolLibrary\library.json`. За да върнете примерните
данни, затворете приложението и изтрийте този файл.

## Бизнес правила (настройки в `LibrarySettings`)

Срок 14 дни · лимит 3 книги за ученик, 5 за учител · блокиране при просрочена книга ·
без второ копие на същото заглавие · справочните издания не се изнасят ·
повредена или загубена книга излиза от наличността · при просрочие над 30 дни редът
се маркира „за разглеждане“.

## Още не е направено (следващи етапи)

- Парола и роли с реална защита (NFR-04, NFR-07) – сега входът е без парола.
- Редактиране на съществуваща книга (има добавяне и премахване).
- Удължаване на срок, резервации, импорт от CSV/Excel (Should от Етап 2).
- **Истински ИИ модел.** `LocalAiSearchService` разпознава жанр, клас и думи и работи без интернет.
  За истински модел се пише нов клас, който имплементира `IAiSearchService`, и се сменя
  един ред в `App.xaml.cs`. Правилата за него са описани в коментара над интерфейса.
- Автоматични тестове на `LibraryService`.
