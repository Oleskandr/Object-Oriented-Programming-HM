using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Xml.Linq;
using System.Text;

// Клас для відображення запису ліків
public class CureRecord
{
    public string CureId { get; set; }
    public string CureName { get; set; }
    public string Group { get; set; }
    public decimal Price { get; set; }
    public int ShelfLife { get; set; }
    public DateTime ProductionDate { get; set; }
}

// Клас-мапа для вказання назв полів у CSV
public sealed class CureRecordMap : ClassMap<CureRecord>
{
    public CureRecordMap()
    {
        Map(m => m.CureId).Name("CureId");
        Map(m => m.CureName).Name("CureName");
        Map(m => m.Group).Name("Group");
        Map(m => m.Price).Name("Price");
        Map(m => m.ShelfLife).Name("ShelfLife");
        Map(m => m.ProductionDate).Name("ProductionDate");
    }
}


// Демонстраційний код
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = UTF8Encoding.UTF8;
        if (args.Length < 4)
        {
            Console.WriteLine("Синтаксис: DemoCsvApp.exe <ВхіднийCsvФайл> " + "<ВихіднийXmlФайл> [<Поле1> <Поле2> ...]");
            return;
        }

        string inputCsvFile = args[0];
        string outputXmlFile = args[1];
        List<string> fieldsToInclude = args.Length > 2 ? new List<string>(args[2..]) : null;

        try
        {
            ConvertCsvToXml(inputCsvFile, outputXmlFile, fieldsToInclude);
            Console.WriteLine("Перетворення виконано успішно.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Виникла помилка: {ex.Message}");
        }

        Console.ReadLine();
    }

    static void ConvertCsvToXml(string inputCsvFile, string outputXmlFile,
                                List<string> fieldsToInclude)
    {
        using (var reader = new StreamReader(inputCsvFile))
        using (var csv = new CsvReader
            (reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            // Додаємо реєстрацію клас-мапи
            csv.Context.RegisterClassMap<CureRecordMap>();

            var records = csv.GetRecords<CureRecord>();

            XElement root = new XElement("записи");

            int rowNumber = 1;
            foreach (var record in records)
            {
                XElement recordElement = new XElement("запис",
                    new XAttribute("Рядок", rowNumber));

                foreach (var field in csv.HeaderRecord)
                {
                    if (fieldsToInclude == null ||
                        fieldsToInclude.Contains(field))
                    {
                        recordElement.Add(new XElement(field,
                              record.GetType().GetProperty(field).
                                     GetValue(record)));
                    }
                }

                root.Add(recordElement);
                rowNumber++;
            }

            XDocument xmlDocument = new XDocument(root);
            xmlDocument.Save(outputXmlFile);
        }
    }
}
