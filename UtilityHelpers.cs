using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace HabitLogger.JonesKwameOsei;

internal class UtilityHelpers
{
    internal void DBConnectionErrorMessage()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[red]❌ Database Error: Unable to connect to the database. Please check your connection string and try again.[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBCreateTableErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error while creating table: {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }
    
    internal void DBAddRecordErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error occurred while saving your walking record:[/]");
        AnsiConsole.MarkupLine($"[red]   {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine($"[yellow]⚠️  Please try again or contact support if the problem persists.[/]");
    }

    // operational error messages for database operations
    internal void DBOperationErrorMessage(Exception invEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Connection error: Unable to connect to the database.[/]");
        AnsiConsole.MarkupLine($"[red]   {invEx.Message}[/]");
        AnsiConsole.MarkupLine($"[yellow]⚠️  Please check your database configuration.[/]");
    }

    internal void DBLoadDataErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error while loading records: {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBInsertErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error while inserting record: {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBUpdateErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error while updating record: {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBDeleteErrorMessage(Exception sqlEx)
    {
        AnsiConsole.MarkupLine($"[red]❌ Database error while deleting record: {sqlEx.Message}[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBIdRecordNotFoundMessage()
    {
        AnsiConsole.MarkupLine("[red]❌ Record not found. Please check the ID and try again.[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void DBNoRecordsFoundMessage()
    {
        AnsiConsole.MarkupLine("[yellow]⚠️ No records found.[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }
   
    internal void DBInputErrorMessage()
    {
        AnsiConsole.MarkupLine("[red]❌ Invalid ID. Please enter a positive number.[/]");
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    internal void HandleSpecialException(Exception ex)
    {
        switch (ex)
        {
            case SqliteException sqlEx:
                AnsiConsole.MarkupLine($"[red]❌ Database error: {sqlEx.Message}[/]");
                AnsiConsole.MarkupLine($"[dim]Error Code: {sqlEx.SqliteErrorCode}[/]");
                break;
            case InvalidOperationException ioEx:
                AnsiConsole.MarkupLine($"[red]❌ Input/Operation error: {ioEx.Message}[/]");
                break;
            default:
                AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred: {ex.Message}[/]");
                AnsiConsole.MarkupLine($"[dim]Error Type: {ex.GetType().Name}[/]");
                break;
        }
    }
}
