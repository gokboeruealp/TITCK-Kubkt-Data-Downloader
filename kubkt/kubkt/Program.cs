using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

class Program
{
    static void Main()
    {
        string dosyaYolu = "kubkt.html";

        HtmlDocument doc = new HtmlDocument();
        doc.Load(dosyaYolu);

        HtmlNode table = doc.DocumentNode.SelectSingleNode("//table[@id='posts']");

        if (table != null)
        {
            var tbody = table.SelectSingleNode("tbody");
            var trs = tbody.SelectNodes("tr");

            Directory.CreateDirectory("KUBKT");
            Directory.SetCurrentDirectory("KUBKT");

            foreach (var tr in trs)
            {
                var tds = tr.SelectNodes("td");

                Medicine medicine = new Medicine();

                medicine.MedicineName = tds[0].InnerText;
                medicine.ActiveIngredientName = tds[1].InnerText;
                medicine.CompanyName = tds[2].InnerText;
                medicine.KubApprovalDate = DateTime.Parse(tds[3].InnerText);
                medicine.KtApprovalDate = DateTime.Parse(tds[4].InnerText);

                var kubDoc = tds[5].SelectSingleNode("div").SelectSingleNode("a");
                var ktDoc = tds[6].SelectSingleNode("div").SelectSingleNode("a");

                if (kubDoc != null)
                {
                    medicine.KubDocument = kubDoc.Attributes["href"].Value;
                }
                if (ktDoc != null)
                {
                    medicine.KtDocument = ktDoc.Attributes["href"].Value;
                }

                string invalidChars = new string(Path.GetInvalidFileNameChars());
                foreach (char invalidChar in invalidChars)
                {
                    medicine.MedicineName = medicine.MedicineName.Replace(invalidChar.ToString(), "-");
                }
                string shortenedName = medicine.MedicineName;
                string folderName = shortenedName;

                if (folderName.Length > 50)
                {
                    folderName = folderName.Substring(0, 50);
                    longNameLogger(folderName);
                }

                Directory.CreateDirectory(folderName);
                Directory.SetCurrentDirectory(folderName);

                string fileName = "KubDocument.pdf";
                if (medicine.KubDocument != null)
                {
                    DownloadFile(medicine.KubDocument, fileName, shortenedName);
                }
                fileName = "KtDocument.pdf";
                if (medicine.KtDocument != null)
                {
                    DownloadFile(medicine.KtDocument, fileName, shortenedName);
                }

                Directory.SetCurrentDirectory("..");
            }
        }
        else
        {
            Console.WriteLine("Table not found.");
        }
    }

    private static void DownloadFile(string url, string fileName, string medicineName)
    {
        try
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, fileName);
            }
        }
        catch (Exception)
        {
            ExceptionFileNameLogger(fileName, url, medicineName);
        }
    }

    private static void ExceptionFileNameLogger(string fileName, string url, string medicineName)
    {
        string logFileName = "notexists_log.txt";
        try
        {
            using (StreamWriter writer = new StreamWriter(logFileName, true))
            {
                writer.WriteLine($"{fileName} {url} {medicineName} NOT EXISTS");
                Console.WriteLine($"{fileName} {url} {medicineName} NOT EXISTS");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Error log file could not be written.");
        }
    }

    private static void longNameLogger(string medicineName)
    {
        string logFileName = "longname_log.txt";
        try
        {
            using (StreamWriter writer = new StreamWriter(logFileName, true))
            {
                writer.WriteLine($"{medicineName} is too long");
                Console.WriteLine($"{medicineName} is too long");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Error log file could not be written.");
        }
    }
}

public class Medicine
{
    public string? MedicineName { get; set; }
    public string? ActiveIngredientName { get; set; }
    public string? CompanyName { get; set; }
    public DateTime? KubApprovalDate { get; set; }
    public DateTime? KtApprovalDate { get; set; }
    public string? KubDocument { get; set; }
    public string? KtDocument { get; set; }

    public override string ToString()
    {
        return $"{MedicineName} {ActiveIngredientName} {CompanyName} {KubApprovalDate} {KtApprovalDate} {KubDocument} {KtDocument}";
    }
}