using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using N_Journal_Tumyanghang_Lawoti.Models;
using Markdig;
using QuestPDFColors = QuestPDF.Helpers.Colors;

namespace N_Journal_Tumyanghang_Lawoti.Services;

public class PdfExportService
{
    public byte[] GeneratePdf(List<JournalEntry> entries, string username)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(QuestPDFColors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Text($"Journal Entries - {username}")
                    .SemiBold().FontSize(16).AlignCenter();

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(20);

                        foreach (var entry in entries)
                        {
                            column.Item().Element(container =>
                            {
                                container
                                    .Border(1)
                                    .BorderColor(QuestPDFColors.Grey.Lighten2)
                                    .Padding(15)
                                    .Column(col =>
                                    {
                                        // Title
                                        col.Item().Text(entry.Title).FontSize(14).SemiBold();
                                        
                                        // Date
                                        col.Item().Text(entry.EntryDate.ToString("MMMM dd, yyyy"))
                                            .FontSize(10)
                                            .FontColor(QuestPDFColors.Grey.Darken1);

                                        // Moods
                                        var moods = new List<string> { entry.PrimaryMood };
                                        if (!string.IsNullOrEmpty(entry.SecondaryMood1))
                                            moods.Add(entry.SecondaryMood1);
                                        if (!string.IsNullOrEmpty(entry.SecondaryMood2))
                                            moods.Add(entry.SecondaryMood2);

                                        col.Item().PaddingTop(5).Text($"Moods: {string.Join(", ", moods)}")
                                            .FontSize(9)
                                            .FontColor(QuestPDFColors.Blue.Darken1);

                                        // Tags
                                        if (entry.JournalEntryTags.Any())
                                        {
                                            var tags = string.Join(", ", entry.JournalEntryTags.Select(jet => jet.Tag.Name));
                                            col.Item().PaddingTop(2).Text($"Tags: {tags}")
                                                .FontSize(9)
                                                .FontColor(QuestPDFColors.Grey.Darken1);
                                        }

                                        // Content
                                        col.Item().PaddingTop(10).Text(StripMarkdown(entry.Content))
                                            .FontSize(10)
                                            .AlignLeft();

                                        // Word count
                                        col.Item().PaddingTop(5).Text($"Word count: {entry.WordCount}")
                                            .FontSize(8)
                                            .FontColor(QuestPDFColors.Grey.Medium)
                                            .AlignRight();
                                    });
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(style => style.FontSize(8).FontColor(QuestPDFColors.Grey.Medium))
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        })
        .GeneratePdf();
    }

    private string StripMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
        
        // Simple markdown stripping - remove markdown syntax
        var text = markdown;
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*\*([^*]+)\*\*", "$1"); // Bold
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\*([^*]+)\*", "$1"); // Italic
        text = System.Text.RegularExpressions.Regex.Replace(text, @"#+\s*", ""); // Headings
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\[([^\]]+)\]\([^\)]+\)", "$1"); // Links
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^-\s*", "", System.Text.RegularExpressions.RegexOptions.Multiline); // Lists
        
        return text;
    }
}

