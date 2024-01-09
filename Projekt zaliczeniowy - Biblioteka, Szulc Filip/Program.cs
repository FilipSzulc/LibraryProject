using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

// Definition of book class
public class Book
{
    // Properties of the Book class
    public string Title { get; private set; }
    public string Author { get; private set; }
    public bool IsAvailable { get; private set; }
    public string RentedByName { get; private set; }

    [JsonIgnore]
    public User RentedBy { get; private set; }
    // Returns the name of the user who rented the book
    public string RentedByUserName => RentedBy?.Name; 
    // Constructor for the Book class
    public Book(string title, string author, bool isAvailable)
    {
        Title = title;
        Author = author;
        IsAvailable = isAvailable;
    }
    // Toggles the availability status of the book
    public void ToggleAvailability()
    {
        IsAvailable = !IsAvailable;
    }
    // Checks out the book to a user
    public void CheckOut(User user)

    {
        IsAvailable = false;
        RentedBy = user;
        RentedByName = user.Name; // it has to save a name of user
    }

    
    // Checks in the book, making it available again
    public void CheckIn()
    {
        IsAvailable = true;
        RentedBy = null;
        RentedByName = null; // Delete a RentedName becuase book can be rented again
    }

}


// Definition of Ebook Class. There was an Idea to make it work, but i didnt have enough time to make it 
public class EBook : Book
{
    public string DownloadLink { get; private set; }

    public EBook(string title, string author, string downloadLink, bool isAvailable)
    : base(title, author, isAvailable) 
    {
        DownloadLink = downloadLink;
    }

}

// Definition of User Class
public class User
{
    public string Name { get; private set; }
    private List<Book> rentedBooks;
    // Constructor for the User class
    public User(string name)
    {
        Name = name;
        rentedBooks = new List<Book>();
    }
    // Allows a user to rent a book
    public bool RentBook(Book book)
    {
        if (rentedBooks.Count >= 5)
        {
            Console.WriteLine("Osiągnięto limit wypożyczonych książek (5). Musisz najpierw oddać jakąś książkę, zanim wypożyczysz kolejną.");
            return false;
        }

        rentedBooks.Add(book);
        return true;
    }
    // Allows a user to return a rented book
    public void ReturnBook(Book book)
    {
        rentedBooks.Remove(book);
    }
    // Retrieves a list of books rented by the user
    public IEnumerable<Book> GetRentedBooks()
    {
        return rentedBooks.AsReadOnly();
    }
}


// Definition of Library Class
public class Library


{
    private List<Book> books;
    private List<User> users;
    // Constructor for the Library class
    public Library()
    {
        books = new List<Book>();
        users = new List<User>();
    }
    // Retrieves or creates a user based on the name
    public User GetUser(string name)
    {
        var user = users.SingleOrDefault(u => u.Name == name);
        if (user == null)
        {
            user = new User(name);
            users.Add(user);
        }
        return user;
    }
    // Creates a new user
    private User CreateUser(string name)
    {
        var newUser = new User(name);
        users.Add(newUser);
        return newUser;
    }
    // Utility method to write text centered in the console
    public static void WriteCentered(string text)
    {
        int windowWidth = Console.WindowWidth;
        int textLength = text.Length;
        int leftPadding = (windowWidth - textLength) / 2;
        leftPadding = Math.Max(leftPadding, 0);
        Console.SetCursorPosition(leftPadding, Console.CursorTop);
        Console.WriteLine(text);
    }
    // Checks out a book to a user
    public void CheckOutBook(string title, string userName)
    {
        var book = FindBook(title);
        var user = GetUser(userName);
        if (book != null && book.IsAvailable)
        {
            if (user.RentBook(book))
            {
                book.CheckOut(user); // Przekaż obiekt user do książki
                WriteCentered($"{title} została wypożyczona przez {userName}.");
            }
            book.CheckOut(user);
        }
        else if (book != null && !book.IsAvailable)
        {
            WriteCentered($"{title} jest już wypożyczona.");
        }
        else
        {
            WriteCentered($"Książka o tytule: {title}, nie znajduje się w bibliotece.");
        }
    }

    // Returns a rented book to the library
    public void ReturnBook(string title)
    {
        var book = FindBook(title);
        if (book != null && !book.IsAvailable)
        {
            book.CheckIn(); // Zaktualizuj status książki
            WriteCentered($"{title} została oddana do biblioteki.");
        }
        else
        {
            WriteCentered($"Książka - {title} - nie może być oddana (nie została wypożyczona, albo nie ma jej w bibliotece).");
        }
    }
    // Displays all books in the library
    public void DisplayAllBooks()
    {
        Console.WriteLine(" ");
        Console.WriteLine(" ");
        WriteCentered("Lista (Alfabetyczna) książek w bibliotece:");
        Console.WriteLine(" ");

        var sortedBooks = books.OrderBy(book => book.Title).ToList(); // Sortowanie książek po tytule

        foreach (var book in sortedBooks)
        {
            string availableText = book.IsAvailable ? "dostępna" : "wypożyczona";

            WriteCentered("-----------------------------------------------------------------------------------------------");
            WriteCentered($"| Tytuł: {book.Title} | Autor/ka: {book.Author} | Dostępność: {availableText} |");
        }
        WriteCentered("-----------------------------------------------------------------------------------------------");

    }

    public void DisplayRentedBooks(string username, string password)
    {
        if (username == "admin" && password == "admin")
        {
            WriteCentered("Lista wypożyczonych książek w bibliotece:");
            bool foundRentedBooks = false;
            foreach (var book in books)
            {
                if (!book.IsAvailable) // Sprawdzamy, czy książka jest wypożyczona
                {
                    foundRentedBooks = true;
                    string rentedByText = book.RentedBy?.Name ?? "nieznany użytkownik";
                    WriteCentered($"Tytuł: {book.Title} | Autor/ka: {book.Author} | Wypożyczona przez: {rentedByText}");
                    WriteCentered("------------------------------------------------------------");
                }
            }

            if (!foundRentedBooks)
            {
                WriteCentered("Brak wypożyczonych książek.");
            }
        }
        else
        {
            WriteCentered("Nieprawidłowe dane logowania, powrót do menu.");
        }
    }



    // Adds a book to the library
    public void AddBook(Book book)
    {
        // Chechs if the book exist in Library 
        if (books.Any(b => b.Title.Equals(book.Title, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine(" ");
        }
        else
        {
            books.Add(book);
        }
    }

    // Finds a book in the library based on the title
    public Book FindBook(string title)
    {
        // Use StringComparison.OrdinalIgnoreCase to ignore case sensitivity of letters
        return books.FirstOrDefault(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }


    // Saving methods

    // Saves the library's books to a JSON file
    public void SaveToJSON(string filePath)
    {
        
        var booksToSerialize = books.Select(book => new
        {
            book.Title,
            book.Author,
            book.IsAvailable,
            book.RentedByName 
        }).ToList();

        string json = JsonConvert.SerializeObject(booksToSerialize, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    // Loads books from a JSON file into the library
    public void LoadFromJSON(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var booksData = JsonConvert.DeserializeObject<List<dynamic>>(json);
            books.Clear();
            foreach (var bookData in booksData)
            {
                var book = new Book((string)bookData.Title, (string)bookData.Author, (bool)bookData.IsAvailable);
                if (!(bool)bookData.IsAvailable)
                {
                    var rentedByName = (string)bookData.RentedByName;
                    if (!string.IsNullOrEmpty(rentedByName))
                    {
                        var rentingUser = GetUser(rentedByName);
                        book.CheckOut(rentingUser);
                    }
                }
                books.Add(book);
            }
        }
    }


    // Saves the library's users and their rented books to a JSON file
    public void SaveUsersToJSON(string filePath)
    {
        var usersData = users.Select(u => new
        {
            u.Name,
            RentedBooks = u.GetRentedBooks().Select(b => b.Title).ToList()
        }).ToList();

        string json = JsonConvert.SerializeObject(usersData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
    // Loads users and their rented books from a JSON file into the library
    public void LoadUsersFromJSON(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            var usersData = JsonConvert.DeserializeObject<List<dynamic>>(json);

            foreach (var userData in usersData)
            {
                User user = GetUser((string)userData.Name);
                foreach (string bookTitle in userData.RentedBooks)
                {
                    Book book = FindBook(bookTitle);
                    if (book != null && book.IsAvailable)
                    {
                        user.RentBook(book);
                        book.CheckOut(user);
                    }
                }

            }
        }
    }
}
// Main program class
class Program
{
    // again static method to write text centered in the console window
    public static void WriteCentered(string text)
    {
        int windowWidth = Console.WindowWidth;
        int textLength = text.Length;
        int leftPadding = (windowWidth - textLength) / 2;
        leftPadding = Math.Max(leftPadding, 0);
        Console.SetCursorPosition(leftPadding, Console.CursorTop);
        Console.WriteLine(text);
    }
    // Main entry point for the program
    static void Main(string[] args)
    {
        Library library = new Library();
        const string booksFile = "library.json";
        const string usersFile = "users.json";
        // Load the state of books and users from JSON files
        library.LoadFromJSON(booksFile);
        library.LoadUsersFromJSON(usersFile);

        // Main loop to present options to the user
        while (true)
        {
            WriteCentered(" ");
            WriteCentered(" ");
            WriteCentered("<-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_->");
            WriteCentered("|   1. Dodaj książkę (Możliwość dodania nowej książki do biblioteki)        |");
            WriteCentered("|   2. Zwróć wypożyczoną książkę                                            |");
            WriteCentered("|   3. Wyszukaj książkę, która cię interesuje                               |");
            WriteCentered("|   4. Wyświetl listę książek                                               |");
            WriteCentered("|   5. Sprawdź wypożyczone książki (Administracja)                          |");
            WriteCentered("|   6. Wyjdź                                                                |");
            WriteCentered("<-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_->");
            WriteCentered(" ");
            WriteCentered("Wybór: ");

            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition((Console.WindowWidth / 2) - 2, currentLineCursor);

            // Read user's choice and perform appropriate action
            var choice = Console.ReadLine();

            Console.Clear(); // clears the console
            WriteCentered($"Wybrano opcję {choice}.");

            // Switch statement to handle different menu options
            switch (choice)
            {
                case "1":
                    WriteCentered("Podaj Tytuł:");
                    int currentLineCursor1 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 4, currentLineCursor1);
                    string title = Console.ReadLine();
                    Console.WriteLine(" ");
                    WriteCentered("Podaj autora:");
                    int CLS2 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 4, CLS2);
                    string author = Console.ReadLine();
                    
                    if (string.IsNullOrEmpty(title))
                    {
                        WriteCentered("Nie podano tytułu. Książka nie została dodana.");
                        break;
                    }

                    var existingBook = library.FindBook(title);
                    if (existingBook != null)
                    {
                        WriteCentered($"Książka o tytule '{title}' już istnieje w bibliotece.");
                        break;
                    }
                    WriteCentered($"Książka o tytule '{title}' została dodana do Biblioteki.");
                    library.AddBook(new Book(title, author, true));
                    break;


                case "2":
                    WriteCentered("Podaj tytuł książki, którą chcesz oddać:");
                    int CLS3 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 5, CLS3);
                    string titleToReturn = Console.ReadLine();
                    library.ReturnBook(titleToReturn);
                    break;


                case "3":
                    WriteCentered("Podaj tytuł książki, która cię interesuje:");
                    int CLS4 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 5, CLS4);
                    string searchTitle = Console.ReadLine();
                    Book foundBook = library.FindBook(searchTitle);
                    if (foundBook != null)
                    {
                        WriteCentered($"Znaleziono książkę: {foundBook.Title} autorstwa {foundBook.Author}");
                        WriteCentered("Chcesz ją wypożyczyć?");
                        WriteCentered("1. Tak, chcę wypożyczyć książkę.");
                        WriteCentered("2. Nie, nie chcę wypożyczyć książki.");
                        int CLS5 = Console.CursorTop;
                        Console.SetCursorPosition((Console.WindowWidth / 2) - 2, CLS5);
                        string subChoice = Console.ReadLine();
                        if (subChoice == "1")
                        {
                            WriteCentered("Podaj imię i nazwisko:");
                            int CLS6 = Console.CursorTop;
                            Console.SetCursorPosition((Console.WindowWidth / 2) - 6, CLS6);
                            string userName = Console.ReadLine();
                            library.CheckOutBook(searchTitle, userName);
                            break;
                        }
                        if (subChoice == "2")
                        {
                            break;
                        }
                        if (subChoice != "1" && subChoice != "2")
                        {
                            WriteCentered("Zły wybór. Powrót do Menu.");
                        }


                        else
                        {
                            WriteCentered("Książka nie jest dostępna w wersji ebookowej.");
                        }
                    }
                    else
                    {
                        WriteCentered("Nie posiadamy książki z tym tytułem w naszym systemie.");
                    }

                    break;

                case "4":
                    library.DisplayAllBooks();
                    break;

                case "5":
                    WriteCentered("Musisz się zalogować, aby sprawdzić wypożyczone książki i użytkowników.");
                    WriteCentered("Podaj login:");
                    int CLS7 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 4, CLS7);
                    string Username = Console.ReadLine();
                    WriteCentered("Podaj hasło:");
                    int CLS8 = Console.CursorTop;
                    Console.SetCursorPosition((Console.WindowWidth / 2) - 4, CLS8);
                    string Password = Console.ReadLine();
                    library.DisplayRentedBooks(Username, Password);
                    break;

                case "6":
                    // Save current state of books and users to JSON files and finish the program
                    library.SaveToJSON(booksFile);
                    library.SaveUsersToJSON(usersFile);
                    WriteCentered("Dane zostały zapisane. Program zamknięty.");
                    return; // Program ends
                default:
                    WriteCentered("Zła opcja, wybierz numer od 1-6.");
                    break;
            }

        }
    }
}