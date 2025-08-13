using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Data;
using System.Globalization;

namespace HabitLogger.JonesKwameOsei;

internal class SharedHelpers
{
    /// <summary>
    /// Gets and validates a date input from the user with format dd-MM-yy
    /// </summary>
    /// <param name="message">The prompt message to display</param>
    /// <returns>Valid date string or "0" if user cancels</returns>
    internal string GetDate(string message)
    {
        string dateInput;
        do
        {
            dateInput = AnsiConsole.Ask<string>($"[green]{message}[/]");

            if (dateInput == "0") return "0";

            if (DateTime.TryParseExact(dateInput, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                return dateInput;
            }

            AnsiConsole.MarkupLine("[red]❌ Invalid date format. Please enter a valid date (dd-MM-yy).[/]");
        } while (true);
    }

    /// <summary>
    /// Gets and validates a date input with a default value
    /// </summary>
    /// <param name="prompt">The prompt message</param>
    /// <param name="defaultValue">Default date value</param>
    /// <returns>Valid date string</returns>
    internal string GetValidDate(string prompt, string defaultValue)
    {
        while (true)
        {
            try
            {
                string input = AnsiConsole.Ask(prompt, defaultValue);

                if (DateTime.TryParseExact(input, "dd-MM-yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return input;
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Invalid date format. Please use dd-MM-yy format.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Input error: {ex.Message}. Please try again.[/]");
            }
        }
    }

    /// <summary>
    /// Prompts user to choose whether to view weekly progress report
    /// </summary>
    /// <returns>True if user wants to see weekly report, false otherwise</returns>
    internal bool PromptForWeeklyProgressReport()
    {
        string choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\n📊 Would you like to view the weekly progress report?")
                .AddChoices("Yes", "No")
        );

        return choice == "Yes";
    }

    /// <summary>
    /// Creates a text-based progress bar
    /// </summary>
    /// <param name="percentage">Progress percentage (0.0 to 1.0)</param>
    /// <param name="width">Width of the progress bar</param>
    /// <param name="fillChar">Character for filled portion</param>
    /// <param name="emptyChar">Character for empty portion</param>
    /// <returns>Progress bar string</returns>
    internal string CreateProgressBar(double percentage, int width = 30, char fillChar = '█', char emptyChar = '░')
    {
        percentage = Math.Clamp(percentage, 0.0, 1.0);
        int filledBars = (int)(percentage * width);
        return "[" + new string(fillChar, filledBars).PadRight(width, emptyChar) + "]";
    }

    /// <summary>
    /// Creates a styled panel for progress display
    /// </summary>
    /// <param name="progressBar">The progress bar content</param>
    /// <param name="header">Panel header text</param>
    /// <param name="borderColor">Border color</param>
    /// <param name="textColor">Text color</param>
    /// <returns>Styled panel</returns>
    internal Panel CreateProgressPanel(string progressBar, string header, Color borderColor, string textColor)
    {
        return new Panel(new Markup($"[{textColor}]{progressBar}[/]"))
            .Header(header)
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor)
            .Expand();
    }

    /// <summary>
    /// Gets and validates an integer input within a specified range
    /// </summary>
    /// <param name="prompt">The prompt message</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="minValue">Minimum allowed value</param>
    /// <param name="maxValue">Maximum allowed value</param>
    /// <param name="fieldName">Name of the field for error messages</param>
    /// <returns>Valid integer within range</returns>
    internal int GetValidInteger(string prompt, int defaultValue, int minValue, int maxValue, string fieldName)
    {
        while (true)
        {
            try
            {
                int value = AnsiConsole.Ask(prompt, defaultValue);

                if (value >= minValue && value <= maxValue)
                {
                    return value;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{fieldName} must be between {minValue:N0} and {maxValue:N0}.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Input error: {ex.Message}. Please enter a valid number.[/]");
            }
        }
    }

    /// <summary>
    /// Gets and validates a double input within a specified range
    /// </summary>
    /// <param name="prompt">The prompt message</param>
    /// <param name="defaultValue">Default value</param>
    /// <param name="minValue">Minimum allowed value</param>
    /// <param name="maxValue">Maximum allowed value</param>
    /// <param name="fieldName">Name of the field for error messages</param>
    /// <returns>Valid double within range</returns>
    internal double GetValidDouble(string prompt, double defaultValue, double minValue, double maxValue, string fieldName)
    {
        while (true)
        {
            try
            {
                double value = AnsiConsole.Ask(prompt, defaultValue);

                if (value >= minValue && value <= maxValue)
                {
                    return value;
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]{fieldName} must be between {minValue:F1} and {maxValue:F1}.[/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Input error: {ex.Message}. Please enter a valid number.[/]");
            }
        }
    }

    /// <summary>
    /// Shows a simple table of records for selection purposes
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="tableName">Name of the database table</param>
    /// <param name="columns">Columns to select and display</param>
    /// <param name="tableTitle">Title for the table</param>
    /// <param name="borderColor">Table border color</param>
    internal void ShowRecordsForSelection(string connectionString, string tableName, string[] columns, string tableTitle, Color borderColor)
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            string columnList = string.Join(", ", columns);
            using (SqliteCommand selectCmd = connection.CreateCommand())
            {
                selectCmd.CommandText = $"SELECT {columnList} FROM {tableName} ORDER BY Date DESC LIMIT 10";
                using (SqliteDataReader reader = selectCmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        AnsiConsole.MarkupLine("[red]❌ No records found.[/]");
                        return;
                    }

                    Table table = new Table()
                        .Border(TableBorder.Simple)
                        .BorderColor(borderColor);

                    // Add columns to table
                    foreach (string column in columns)
                    {
                        table.AddColumn($"[bold]{column}[/]");
                    }

                    while (reader.Read())
                    {
                        string[] rowData = new string[columns.Length];
                        for (int i = 0; i < columns.Length; i++)
                        {
                            if (reader.IsDBNull(columns[i]))
                            {
                                rowData[i] = "[dim]No data[/]";
                            }
                            else
                            {
                                Object? value = reader[columns[i]];
                                rowData[i] = value switch
                                {
                                    int intVal => intVal.ToString("N0"),
                                    double doubleVal => doubleVal.ToString("F2"),
                                    string strVal => strVal ?? string.Empty,
                                    _ => value?.ToString() ?? string.Empty
                                };
                            }
                        }
                        table.AddRow(rowData);
                    }

                    AnsiConsole.Write(table);
                }
            }
        }
    }

    /// <summary>
    /// Waits for user input with a custom message
    /// </summary>
    /// <param name="message">Message to display</param>
    internal void WaitForKeyPress(string message = "Press any key to continue...")
    {
        AnsiConsole.MarkupLine($"\n[dim]{message}[/]");
        Console.ReadKey();
    }

    /// <summary>
    /// Clears console and waits for user input
    /// </summary>
    /// <param name="message">Message to display before waiting</param>
    internal void ClearConsoleAndWait(string message = "Press any key to continue...")
    {
        WaitForKeyPress(message);
        Console.Clear();
    }
}