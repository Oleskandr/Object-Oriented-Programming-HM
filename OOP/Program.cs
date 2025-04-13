using System;
using System.Collections.Generic;
using System.IO;

// Інтерфейс для всіх ліків
public interface ICure
{
    void DisplayInfo();
    bool IsExpired();
}

// Інтерфейс для логування
public interface ILogger
{
    void Log(string message);
}

// Базовий клас для вітчизняних ліків
public class Cure : ICure, IComparable<Cure>
{
    public string Name { get; set; }
    public string Group { get; set; }
    public decimal Price { get; set; }
    public int ShelfLife { get; set; }
    public DateTime ProductionDate { get; set; }

    public Cure(string name, string group, decimal price, int shelfLife, DateTime productionDate)
    {
        Name = name;
        Group = group;
        Price = price;
        ShelfLife = shelfLife;
        ProductionDate = productionDate;
    }

    public virtual void DisplayInfo()
    {
        Console.WriteLine($"Назва: {Name}");
        Console.WriteLine($"Група: {Group}");
        Console.WriteLine($"Ціна: {Price} грн");
        Console.WriteLine($"Термін зберігання: {ShelfLife} місяців");
        Console.WriteLine($"Дата виробництва: {ProductionDate.ToShortDateString()}");
        Console.WriteLine($"Придатний до: {GetExpiryDate().ToShortDateString()}");

        if (IsExpired())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("УВАГА: Термін придатності закінчився!");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"Залишилось днів: {(GetExpiryDate() - DateTime.Now).Days}");
        }
    }

    public DateTime GetExpiryDate()
    {
        return ProductionDate.AddMonths(ShelfLife);
    }

    public bool IsExpired()
    {
        return DateTime.Now > GetExpiryDate();
    }

    // Реалізація інтерфейсу IComparable для сортування ліків за ціною
    public int CompareTo(Cure other)
    {
        if (other == null) return 1;
        return Price.CompareTo(other.Price);
    }
}

// Клас для імпортних ліків
public class ImportCure : Cure
{
    public string Country { get; set; }
    public string CertificateNumber { get; set; }

    public ImportCure(string name, string group, decimal price, int shelfLife,
        DateTime productionDate, string country, string certificateNumber)
        : base(name, group, price, shelfLife, productionDate)
    {
        Country = country;
        CertificateNumber = certificateNumber;
    }

    // Перезаписуємо метод з базового класу
    public override void DisplayInfo()
    {
        Console.WriteLine("Імпортні препарати: ");
        base.DisplayInfo();
        Console.WriteLine($"Країна-виробник: {Country}");
        Console.WriteLine($"Номер сертифікату: {CertificateNumber}");
    }
}

// Класи логерів
public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[LOG] {DateTime.Now}: {message}");
        Console.ResetColor();
    }
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        // Логування в файл (зараз виводиться на консоль для прикладу)
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Log to file: {message}");
        Console.ResetColor();
    }
}

// Клас для управління аптечною базою даних
public class PharmacyManager
{
    private List<Cure> cures;
    private ILogger logger;

    public PharmacyManager(ILogger logger)
    {
        cures = new List<Cure>();
        this.logger = logger;
    }

    public void AddCure(Cure cure)
    {
        cures.Add(cure);
        logger.Log($"Додано новий препарат: {cure.Name}");
    }

    public void DisplayAllCures()
    {
        Console.WriteLine("\nСписок всіх препаратів: ");
        if (cures.Count == 0)
        {
            Console.WriteLine("База даних порожня");
            return;
        }

        foreach (var cure in cures)
        {
            cure.DisplayInfo();
            Console.WriteLine();
        }

        logger.Log("Виведено список всіх препаратів");
    }

    public void SortByPrice()
    {
        cures.Sort();
        logger.Log("Препарати відсортовано за ціною (від найдешевших до найдорожчих)");
    }

    public void SortByExpiryDate()
    {
        cures.Sort((a, b) => a.GetExpiryDate().CompareTo(b.GetExpiryDate()));
        logger.Log("Препарати відсортовано за терміном придатності");
    }

    public List<ImportCure> GetAllImportedCures()
    {
        List<ImportCure> importedCures = new List<ImportCure>();

        foreach (var cure in cures)
        {
            if (cure is ImportCure importCure)
            {
                importedCures.Add(importCure);
            }
        }

        logger.Log($"Знайдено {importedCures.Count} імпортних препаратів");
        return importedCures;
    }
}

// Демонстраційний код
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Створюємо логери
        ILogger consoleLogger = new ConsoleLogger();
        ILogger fileLogger = new FileLogger();

        // Створюємо менеджер аптеки
        PharmacyManager pharmacy = new PharmacyManager(consoleLogger);

        // Створюємо вітчизняні ліки
        Cure analgin = new Cure(
            "Анальгін",
            "Знеболювальні",
            25.50m,
            24,
            DateTime.Now.AddMonths(-12)
        );

        Cure paracetamol = new Cure(
            "Парацетамол",
            "Жарознижувальні",
            32.80m,
            36,
            DateTime.Now.AddMonths(-3)
        );

        Cure validol = new Cure(
            "Валідол",
            "Серцево-судинні",
            15.75m,
            18,
            DateTime.Now.AddMonths(-16)
        );

        // Створюємо імпортні ліки
        ImportCure nurofen = new ImportCure(
            "Нурофен",
            "Знеболювальні",
            120.50m,
            36,
            DateTime.Now.AddMonths(-6),
            "Великобританія",
            "UA-12345-2023"
        );

        ImportCure mezim = new ImportCure(
            "Мезим форте",
            "Травні",
            98.75m,
            24,
            DateTime.Now.AddMonths(-4),
            "Німеччина",
            "UA-67890-2023"
        );

        // Додаємо всі ліки до бази
        pharmacy.AddCure(analgin);
        pharmacy.AddCure(paracetamol);
        pharmacy.AddCure(validol);
        pharmacy.AddCure(nurofen);
        pharmacy.AddCure(mezim);

        // Виводимо всі ліки
        pharmacy.DisplayAllCures();

        // Сортуємо за ціною і виводимо знову
        Console.WriteLine("\nВідсортований за ціною: ");
        pharmacy.SortByPrice();
        pharmacy.DisplayAllCures();

        // Виводимо всі імпортні ліки
        Console.WriteLine("\nТільки імпортні: ");
        List<ImportCure> importedCures = pharmacy.GetAllImportedCures();

        // Демонстрація роботи з логерами
        Console.WriteLine("\nЛогери: ");
        consoleLogger.Log("Це повідомлення від консольного логера");
        fileLogger.Log("Це повідомлення записано у файл");
    }
}