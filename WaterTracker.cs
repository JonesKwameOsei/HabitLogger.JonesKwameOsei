using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitLogger.JonesKwameOsei;

internal class WaterTracker
{
    private const int DAILY_GOAL_GLASSES = 8;
    private readonly string connectionString;
    private readonly SharedHelpers _sharedHelpers = new SharedHelpers();
    private readonly UtilityHelpers _utilityHelpers = new UtilityHelpers();

    internal WaterTracker(string connectionString)
    {
        this.connectionString = connectionString;
    }

    internal void AddRecord()
    {
        try
        {
            Console.Clear();

            Panel panel = new Panel(new Text("💧 LOG WATER INTAKE", new Style(Color.Cyan1, Color.Default, Decoration.Bold)))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1);
            AnsiConsole.Write(panel);
            Console.WriteLine();

            string date = _sharedHelpers.GetDate("\n📅 Enter the date (format: dd-MM-yy) or insert [red]0[/] to Return to Main Menu: \n");
            if (date == "0") return;

            int quantity = _sharedHelpers.GetValidInteger("🥤 Please enter number of glasses drunk (no decimals or negatives allowed) or enter [red]0[/] to Return to the Main Menu.", 0, 0, 50, "Glasses");
            if (quantity == 0) return;

            Console.Clear();
            string description = AnsiConsole.Ask<string>("📝 Please enter a [green]description[/] for this record (optional):");

            // Validate data before database insertion
            if (quantity <= 0)
            {
                AnsiConsole.MarkupLine("[red]❌ Invalid data: Quantity must be greater than zero.[/]");
                _sharedHelpers.WaitForKeyPress();
                return;
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqliteCommand insertCmd = connection.CreateCommand())
                    {
                        insertCmd.CommandText = @"
                        INSERT INTO drinking_water (Date, Quantity, Description, CreatedAt)
                        VALUES ($date, $quantity, $description, $createdAt)";
                        insertCmd.Parameters.AddWithValue("$date", date);
                        insertCmd.Parameters.AddWithValue("$quantity", quantity);
                        insertCmd.Parameters.AddWithValue("$description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description);
                        insertCmd.Parameters.AddWithValue("$createdAt", DateTime.Now);

                        insertCmd.ExecuteNonQuery();
                        AnsiConsole.MarkupLine($"\n[green]✅ Successfully logged {quantity} glasses of water on {date}![/]");
                        ShowTodayProgress(date);
                    }
                }
                catch (SqliteException sqlEx)
                {
                    _utilityHelpers.DBAddRecordErrorMessage(sqlEx);
                }
                catch (InvalidOperationException invEx)
                {
                    _utilityHelpers.DBOperationErrorMessage(invEx);
                }
            }
        }
        catch (FormatException formatEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Date format error: {formatEx.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️  Please ensure you're using the correct date format (dd-MM-yy).[/]");
        }
        catch (ArgumentException argEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Input validation error: {argEx.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️  Please check your input values and try again.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred while adding your water record:[/]");
            AnsiConsole.MarkupLine($"[red]   {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️  Please try again or restart the application.[/]");
        }
        finally
        {
            _sharedHelpers.WaitForKeyPress();
        }
    }

    internal void ViewRecords()
    {
        try
        {
            Console.Clear();

            var panel = new Panel(new Text("📊 WATER INTAKE RECORDS", new Style(Color.Yellow, Color.Default, Decoration.Bold)))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Yellow);
            AnsiConsole.Write(panel);
            Console.WriteLine();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand selectCmd = connection.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT * FROM drinking_water ORDER BY Date DESC, CreatedAt DESC";
                    using (SqliteDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            AnsiConsole.MarkupLine("\n[red]No records found. Start logging your water intake![/]");
                            _sharedHelpers.WaitForKeyPress();
                            return;
                        }

                        var table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.Cyan1)
                            .Title(new TableTitle("Water Intake Records"))
                            .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Date[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Glasses[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Description[/]"))
                            .AddColumn(new TableColumn("[bold]Created At[/]"));

                        while (reader.Read())
                        {
                            int quantity = reader.GetInt32("Quantity");
                            string glassesDisplay = quantity >= DAILY_GOAL_GLASSES ? 
                                $"[bold green]{quantity}[/] 🎯" : $"[cyan]{quantity}[/] 🥤";
                            string description = reader.IsDBNull("Description") ? "[dim]No description[/]" : reader.GetString("Description");

                            table.AddRow(
                                reader.GetInt32("Id").ToString(),
                                reader.GetString("Date"),
                                glassesDisplay,
                                description,
                                reader.GetString("CreatedAt")
                            );
                        }

                        AnsiConsole.Write(table);
                    }
                }
            }

            if (_sharedHelpers.PromptForWeeklyProgressReport())
            {
                ShowWeeklyProgress();
            }
        }
        catch (SqliteException ex)
        {
            _utilityHelpers.DBLoadDataErrorMessage(ex);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred: {ex.Message}[/]");
        }
        finally
        {
            _sharedHelpers.WaitForKeyPress();
        }
    }

    public void DeleteRecord()
    {
        try
        {
            Console.Clear();

            Panel panel = new Panel(new Text("🗑️ DELETE WATER RECORD", new Style(Color.Red, Color.Default, Decoration.Bold)))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Red);
            AnsiConsole.Write(panel);
            Console.WriteLine();

            try
            {
                ShowRecordsForSelection();
            }
            catch (SqliteException sqlEx)
            {
                _utilityHelpers.DBLoadDataErrorMessage(sqlEx);
                return;
            }

            int recordId;
            try
            {
                recordId = AnsiConsole.Ask<int>("Enter the [red]ID[/] of the record you want to delete (or [red]0[/] to cancel):");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Invalid input: {ex.Message}[/]");
                _sharedHelpers.WaitForKeyPress();
                return;
            }

            if (recordId == 0) return;

            if (recordId < 0)
            {
                _utilityHelpers.DBInputErrorMessage();
                return;
            }

            // Confirm deletion
            if (!AnsiConsole.Confirm($"Are you sure you want to delete record ID {recordId}?"))
                return;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqliteCommand deleteCmd = connection.CreateCommand())
                    {
                        deleteCmd.CommandText = "DELETE FROM drinking_water WHERE Id = $id";
                        deleteCmd.Parameters.AddWithValue("$id", recordId);

                        int rowsAffected = deleteCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AnsiConsole.MarkupLine("[green]✅ Record deleted successfully![/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[yellow]⚠️ Record not found! It may have been already deleted.[/]");
                        }
                    }
                }
                catch (SqliteException sqlEx)
                {
                    _utilityHelpers.DBDeleteErrorMessage(sqlEx);
                }
                catch (InvalidOperationException ioEx)
                {
                    _utilityHelpers.DBOperationErrorMessage(ioEx);
                }
            }
        }
        catch (Exception ex)
        {
            _utilityHelpers.HandleSpecialException(ex);
        }
        finally
        {
            _sharedHelpers.WaitForKeyPress();
        }
    }


    private void ShowWeeklyProgress()
    {
        try
        {
            Console.WriteLine();
            var rule = new Spectre.Console.Rule("[yellow]📈 Last 7 Days Water Progress[/]").RuleStyle("yellow");
            AnsiConsole.Write(rule);

            var chart = new BarChart()
                .Width(60)
                .Label("[green bold underline]Daily Water Intake (Glasses)[/]")
                .CenterLabel();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = DateTime.Today.AddDays(-i);
                    string dateStr = date.ToString("dd-MM-yy");

                    using (SqliteCommand selectCmd = connection.CreateCommand())
                    {
                        selectCmd.CommandText = "SELECT SUM(Quantity) FROM drinking_water WHERE Date = $date";
                        selectCmd.Parameters.AddWithValue("$date", dateStr);

                        object? result = selectCmd.ExecuteScalar();
                        int totalGlasses = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                        Color barColor = totalGlasses >= DAILY_GOAL_GLASSES ? Color.Green : Color.Cyan1;
                        chart.AddItem(date.ToString("MMM dd"), totalGlasses, barColor);
                    }
                }
            }

            AnsiConsole.Write(chart);
        }
        catch (SqliteException sqlEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Database error while retrieving weekly progress: {sqlEx.Message}[/]");
        }
        catch (InvalidOperationException ioEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Connection error while retrieving weekly progress: {ioEx.Message}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error retrieving weekly progress: {ex.Message}[/]");
        }
    }


    private void ShowRecordsForSelection()
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand selectCmd = connection.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT Id, Date, Quantity, Description FROM drinking_water ORDER BY Date DESC LIMIT 10";
                    using (SqliteDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            AnsiConsole.MarkupLine("\n[red]❌ No records found.[/]");
                            return;
                        }

                        var table = new Table()
                            .Border(TableBorder.Simple)
                            .BorderColor(Color.DarkSlateGray1)
                            .Title(new TableTitle("Recent Water Records"))
                            .AddColumn("[bold]ID[/]")
                            .AddColumn("[bold]Date[/]")
                            .AddColumn("[bold]Glasses[/]")
                            .AddColumn("[bold]Description[/]");

                        while (reader.Read())
                        {
                            try
                            {
                                int quantity = reader.GetInt32("Quantity");
                                string glassesDisplay = quantity >= DAILY_GOAL_GLASSES ? 
                                    $"[bold green]{quantity}[/] 🎯" : $"[cyan]{quantity}[/] 🥤";
                                string description = reader.IsDBNull("Description") ? "[dim]No description[/]" : reader.GetString("Description");
                                
                                table.AddRow(
                                    reader.GetInt32("Id").ToString(),
                                    reader.GetString("Date"),
                                    glassesDisplay,
                                    description
                                );
                            }
                            catch (Exception rowEx)
                            {
                                AnsiConsole.MarkupLine($"[yellow]⚠️ Error reading record: {rowEx.Message}[/]");
                                // Continue with next record
                                continue;
                            }
                        }

                        AnsiConsole.Write(table);
                    }
                }
            }
        }
        catch (SqliteException sqlEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Database error while loading records: {sqlEx.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️ Unable to display records for selection.[/]");
        }
        catch (InvalidOperationException ioEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Connection error while loading records: {ioEx.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️ Please check your database connection.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Unexpected error while displaying records: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️ Unable to display records for selection.[/]");
        }
    }

    private void ShowTodayProgress(string date)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand selectCmd = connection.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT SUM(Quantity) FROM drinking_water WHERE Date = $date";
                    selectCmd.Parameters.AddWithValue("$date", date);
                    using (SqliteDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int totalGlasses = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);

                            AnsiConsole.MarkupLine($"\n[green]Today's Progress for {date}:[/]");
                            AnsiConsole.MarkupLine($"[bold]Total Glasses:[/] [cyan]{totalGlasses:N0}[/] 🥤");
                            AnsiConsole.MarkupLine($"[bold]Total Water:[/] [blue]{totalGlasses * 250:N0}ml[/] 💧");
                            
                            if (totalGlasses >= DAILY_GOAL_GLASSES)
                            {
                                AnsiConsole.MarkupLine("[bold green]🎯 Congratulations! You've reached your daily hydration goal![/]");
                            }
                            else
                            {
                                AnsiConsole.MarkupLine($"[yellow]Keep going! You need [bold]{DAILY_GOAL_GLASSES - totalGlasses:N0}[/] more glasses to reach your daily goal.[/]");
                            }
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[red]❌ No records found for today.[/]");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error retrieving today's progress: {ex.Message}[/]");
        }
    }

}

