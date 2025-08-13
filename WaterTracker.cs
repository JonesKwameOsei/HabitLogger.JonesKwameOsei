using Microsoft.Data.Sqlite;
using Spectre.Console;
using System;
using System.Collections.Generic;
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

    internal WaterTracker(string connectionString)
    {
        this.connectionString = connectionString;
    }

    internal void AddRecord()
    {
        Console.Clear();

        Panel panel = new Panel(new Text("💧� LOG WATER INTAKE", new Style(Color.Cyan1, Color.Black, Decoration.Bold)))
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

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            using (SqliteCommand insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"
                INSERT INTO drinking_water (Date, Quantity, Description)
                VALUES ($date, $quantity, $description)";
                insertCmd.Parameters.AddWithValue("$date", date);
                insertCmd.Parameters.AddWithValue("$quantity", quantity);
                insertCmd.Parameters.AddWithValue("$description", description);
                insertCmd.ExecuteNonQuery();
            }
        }

        AnsiConsole.MarkupLine($"[green]✅ Successfully logged {quantity} glasses of water on {date}.[/]");

        _sharedHelpers.WaitForKeyPress();
    }

    internal void ViewRecords()
    {

    }


}

