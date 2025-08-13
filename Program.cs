using HabitLogger.JonesKwameOsei;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Data;
using System.Globalization;
using System.Reflection.PortableExecutable;

string connectionString = "Data Source=habit-Tracker.db";

CreateDatabase();

MainMenu();

void MainMenu()
{
  WaterTracker waterTracker = new WaterTracker(connectionString);
  WalkTracker walkingTracker = new WalkTracker(connectionString);
  Helpers header = new Helpers();

  bool isMenuRunning = true;

  while (isMenuRunning)
  {
    Console.Clear();
    header.PrintMainHeader();

    string habitChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("🎯 [bold green]Which habit would you like to track?[/]")
            .AddChoices(
                "💧 Water Intake",
                "🚶‍♂️ Walking Activity",
                "📊 View All Statistics",
                "❌ Quit Application"));

    try
    {
      switch (habitChoice)
      {
        case "💧 Water Intake":
          WaterMenu(waterTracker);
          break;
        case "🚶‍♂️ Walking Activity":
          WalkingMenu(walkingTracker);
          break;
        case "📊 View All Statistics":
          ShowCombinedStatistics(waterTracker, walkingTracker);
          ClearConsoleAndWait();
          break;
        case "❌ Quit Application":
          Console.Clear();
          AnsiConsole.MarkupLine("[green]👋 Thank you for using the Habit Logger![/]");
          isMenuRunning = false;
          break;
        default:
          WarningMessage();
          ClearConsoleAndWait();
          break;
      }
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"[red]❌ Critical error: {ex.Message}[/]");
      ClearConsoleAndWait("Press any key to continue...");
    }
  }
}

void WaterMenu(WaterTracker waterTracker)
{
  //WaterTracker waterTracker = new WaterTracker(connectionString);
  Helpers header = new Helpers();
  header.PrintWaterTrackerHeader();
  bool isWaterMenuRunning = true;

  while (isWaterMenuRunning)
  {
    Console.Clear();

    header.PrintWaterTrackerHeader();
    var panel = new Panel(new Text("💧 WATER INTAKE TRACKER", new Style(Color.Cyan1, Color.Default, Decoration.Bold)))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1);
    AnsiConsole.Write(panel);

    var usersChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices(
                "➕ Add Water Record",
                "🗑️ Delete Water Record",
                "👁️ View Water Records",
                "✏️ Update Water Record",
                "📊 Show Water Statistics",
                "🔙 Back to Main Menu"));
    try
    {
      switch (usersChoice)
      {
        case "➕ Add Water Record":
          waterTracker.AddRecord();
          break;
        case "🗑️ Delete Water Record":
          //waterTracker.DeleteRecord();
          break;
        case "👁️ View Water Records":
          waterTracker.ViewRecords();
          break;
        case "✏️ Update Water Record":
          //waterTracker.UpdateRecord();
          break;
        case "📊 Show Water Statistics":
          //waterTracker.ShowStatistics();
          break;
        case "🔙 Back to Main Menu":
          isWaterMenuRunning = false;
          Console.Clear();
          break;
        default:
          WarningMessage();
          ClearConsoleAndWait();
          break;

      }
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
      ClearConsoleAndWait("Press any key to return to menu...");
    }

  }
}

void WalkingMenu(WalkTracker walkingTracker)
{

  Helpers header = new();
  header.PrintWalkingTrackerHeader();

  bool isWalkingMenuRunning = true;

  while (isWalkingMenuRunning)
  {
    Console.Clear();
    header.PrintWalkingTrackerHeader();

    Panel panel = new Panel(new Text("🚶‍ WALKING ACTIVITY TRACKER", new Style(Color.Green, Color.Default, Decoration.Bold)))
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Green);
    AnsiConsole.Write(panel);

    String usersChoice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What would you like to do?")
            .AddChoices(
                "➕ Add Walking Record",
                "🗑️  Delete Walking Record",
                "👁️  View Walking Records",
                "✏️  Update Walking Record",
                "📊  Show Walking Statistics",
                "🔙  Back to Main Menu"));

    try
    {
      switch (usersChoice)
      {
        case "➕ Add Walking Record":
          walkingTracker.AddRecord();
          break;
        case "🗑️  Delete Walking Record":
          walkingTracker.DeleteRecord();
          break;
        case "👁️  View Walking Records":
          walkingTracker.ViewRecord();
          break;
        case "✏️  Update Walking Record":
          walkingTracker.UpdateRecord();
          break;
        case "📊  Show Walking Statistics":
          walkingTracker.ShowStatistics();
          break;
        case "🔙  Back to Main Menu":
          isWalkingMenuRunning = false;
          Console.Clear();
          break;
        default:
          WarningMessage();
          ClearConsoleAndWait();
          break;
      }
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
      ClearConsoleAndWait("Press any key to return to menu...");
    }
  }
}

void ShowCombinedStatistics(WaterTracker waterTracker, WalkTracker walkingTracker)
{
  try
  {
    Console.Clear();

    var panel = new Panel(new Text("📊 COMBINED HEALTH STATISTICS", new Style(Color.Magenta1, Color.Black, Decoration.Bold)))
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Magenta1);
    AnsiConsole.Write(panel);
    Console.WriteLine();

    var layout = new Layout("Root")
        .SplitColumns(
            new Layout("Water").Size(50),
            new Layout("Walking").Size(50)
        );

    Panel waterPanel;
    try
    {
      var waterStats = GetWaterStatistics();
      waterPanel = CreateWaterStatsPanel(waterStats);
    }
    catch (SqliteException sqlEx)
    {
      waterPanel = new Panel(new Markup("[red]❌ Database error loading water stats[/]\n" +
                                      $"[dim]{sqlEx.Message}[/]"))
          .Header("💧 Water Statistics")
          .Border(BoxBorder.Rounded)
          .BorderColor(Color.Red);
    }
    catch (Exception ex)
    {
      waterPanel = new Panel(new Markup("[red]❌ Error loading water stats[/]\n" +
                                      $"[dim]{ex.Message}[/]"))
          .Header("💧 Water Statistics")
          .Border(BoxBorder.Rounded)
          .BorderColor(Color.Red);
    }

    Panel walkingPanel;
    try
    {
      var walkingStats = GetWalkingStatistics();
      walkingPanel = CreateWalkingStatsPanel(walkingStats);
    }
    catch (SqliteException sqlEx)
    {
      walkingPanel = new Panel(new Markup("[red]❌ Database error loading walking stats[/]\n" +
                                        $"[dim]{sqlEx.Message}[/]"))
          .Header("🚶‍♂️ Walking Statistics")
          .Border(BoxBorder.Rounded)
          .BorderColor(Color.Red);
    }
    catch (Exception ex)
    {
      walkingPanel = new Panel(new Markup("[red]❌ Error loading walking stats[/]\n" +
                                        $"[dim]{ex.Message}[/]"))
          .Header("🚶‍♂️ Walking Statistics")
          .Border(BoxBorder.Rounded)
          .BorderColor(Color.Red);
    }

    layout["Water"].Update(waterPanel);
    layout["Walking"].Update(walkingPanel);

    AnsiConsole.Write(layout);

    try
    {
      ShowCombinedInsights();
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"\n[red]❌ Error generating insights: {ex.Message}[/]");
    }

    try
    {
      ShowWeeklyCombinedChart();
    }
    catch (Exception ex)
    {
      AnsiConsole.MarkupLine($"\n[red]❌ Error showing weekly chart: {ex.Message}[/]");
    }
  }
  catch (Exception globalEx)
  {
    // Handle any critical errors in the UI setup
    Console.Clear();
    AnsiConsole.MarkupLine("[red]❌ Critical error displaying combined statistics[/]");
    AnsiConsole.MarkupLine($"[dim]Error: {globalEx.Message}[/]");
    AnsiConsole.MarkupLine("[dim]Error Type: {globalEx.GetType().Name}[/]");
  }
}

// Helper method to get water statistics
(int totalEntries, int totalGlasses, double avgDaily, int goalDays, string bestDay) GetWaterStatistics()
{
  using (SqliteConnection connection = new SqliteConnection(connectionString))
  {
    connection.Open();

    // Total statistics
    using (SqliteCommand totalCmd = connection.CreateCommand())
    {
      totalCmd.CommandText = "SELECT COUNT(*), COALESCE(SUM(Quantity), 0) FROM drinking_water";
      using (SqliteDataReader reader = totalCmd.ExecuteReader())
      {
        reader.Read();
        int totalEntries = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        int totalGlasses = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);

        reader.Close();

        // Average daily consumption (last 30 days)
        double avgDaily = 0;
        using (SqliteCommand avgCmd = connection.CreateCommand())
        {
          avgCmd.CommandText = @"
                        SELECT AVG(daily_total) FROM (
                            SELECT SUM(Quantity) as daily_total 
                            FROM drinking_water 
                            WHERE Date >= date('now', '-30 days')
                            GROUP BY Date
                        )";
          object? avgResult = avgCmd.ExecuteScalar();
          avgDaily = avgResult != DBNull.Value && avgResult != null ? Convert.ToDouble(avgResult) : 0;
        }

        // Goal achievement days
        int goalDays = 0;
        using (SqliteCommand goalCmd = connection.CreateCommand())
        {
          goalCmd.CommandText = @"
                        SELECT COUNT(*) FROM (
                            SELECT Date 
                            FROM drinking_water 
                            GROUP BY Date 
                            HAVING SUM(Quantity) >= 8
                        )";
          object? goalResult = goalCmd.ExecuteScalar();
          goalDays = goalResult != DBNull.Value ? Convert.ToInt32(goalResult) : 0;
        }

        // Best day
        string bestDay = "No data";
        using (SqliteCommand bestCmd = connection.CreateCommand())
        {
          bestCmd.CommandText = @"
                        SELECT Date, SUM(Quantity) as Total 
                        FROM drinking_water 
                        GROUP BY Date 
                        ORDER BY Total DESC 
                        LIMIT 1";
          using (SqliteDataReader bestReader = bestCmd.ExecuteReader())
          {
            if (bestReader.Read())
            {
              string date = bestReader.GetString("Date");
              int amount = bestReader.GetInt32("Total");
              bestDay = $"{date} ({amount} glasses)";
            }
          }
        }

        return (totalEntries, totalGlasses, avgDaily, goalDays, bestDay);
      }
    }
  }
}

// Helper method to get walking statistics
(int totalEntries, int totalSteps, double totalDistance, double avgSteps, int goalDays, string bestDay) GetWalkingStatistics()
{
  using (SqliteConnection connection = new SqliteConnection(connectionString))
  {
    connection.Open();

    using (SqliteCommand totalCmd = connection.CreateCommand())
    {
      totalCmd.CommandText = "SELECT COUNT(*), COALESCE(SUM(Steps), 0), COALESCE(SUM(Distance), 0) FROM walking_activity";
      using (SqliteDataReader reader = totalCmd.ExecuteReader())
      {
        reader.Read();
        int totalEntries = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
        int totalSteps = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
        double totalDistance = reader.IsDBNull(2) ? 0 : reader.GetDouble(2);

        reader.Close();

        // Average daily steps (last 30 days)
        double avgSteps = 0;
        using (SqliteCommand avgCmd = connection.CreateCommand())
        {
          avgCmd.CommandText = @"
                        SELECT AVG(daily_steps) FROM (
                            SELECT SUM(Steps) as daily_steps 
                            FROM walking_activity 
                            WHERE Date >= date('now', '-30 days')
                            GROUP BY Date
                        )";
          object? avgResult = avgCmd.ExecuteScalar();
          avgSteps = avgResult != DBNull.Value && avgResult != null ? Convert.ToDouble(avgResult) : 0;
        }

        // Goal achievement days (10,000 steps)
        int goalDays = 0;
        using (SqliteCommand goalCmd = connection.CreateCommand())
        {
          goalCmd.CommandText = @"
                        SELECT COUNT(*) FROM (
                            SELECT Date 
                            FROM walking_activity 
                            GROUP BY Date 
                            HAVING SUM(Steps) >= 10000
                        )";
          object? goalResult = goalCmd.ExecuteScalar();
          goalDays = goalResult != DBNull.Value ? Convert.ToInt32(goalResult) : 0;
        }

        // Best day
        string bestDay = "No data";
        using (SqliteCommand bestCmd = connection.CreateCommand())
        {
          bestCmd.CommandText = @"
                        SELECT Date, SUM(Steps) as TotalSteps, SUM(Distance) as TotalDistance 
                        FROM walking_activity 
                        GROUP BY Date 
                        ORDER BY TotalSteps DESC 
                        LIMIT 1";
          using (SqliteDataReader bestReader = bestCmd.ExecuteReader())
          {
            if (bestReader.Read())
            {
              string date = bestReader.GetString("Date");
              int steps = bestReader.GetInt32("TotalSteps");
              double distance = bestReader.GetDouble("TotalDistance");
              bestDay = $"{date} ({steps:N0} steps, {distance:F2} km)";
            }
          }
        }

        return (totalEntries, totalSteps, totalDistance, avgSteps, goalDays, bestDay);
      }
    }
  }
}

Panel CreateWaterStatsPanel((int totalEntries, int totalGlasses, double avgDaily, int goalDays, string bestDay) stats)
{
  var content = new Markup($"""
        [cyan]📋 Total entries:[/] {stats.totalEntries:N0}
        [cyan]🥤 Total glasses:[/] {stats.totalGlasses:N0}
        [cyan]💧 Total water:[/] {stats.totalGlasses * 250:N0}ml
        [cyan]📈 Daily average:[/] {stats.avgDaily:F1} glasses
        [cyan]🎯 Goal days:[/] {stats.goalDays:N0}
        [cyan]🏆 Best day:[/] {stats.bestDay}
        """);

  return new Panel(content)
      .Header("💧 Water Statistics")
      .Border(BoxBorder.Rounded)
      .BorderColor(Color.Cyan1);
}

Panel CreateWalkingStatsPanel((int totalEntries, int totalSteps, double totalDistance, double avgSteps, int goalDays, string bestDay) stats)
{
  var content = new Markup($"""
        [green]📋 Total entries:[/] {stats.totalEntries:N0}
        [green]👣 Total steps:[/] {stats.totalSteps:N0}
        [green]🏃‍♂️ Total distance:[/] {stats.totalDistance:F2} km
        [green]📈 Daily average:[/] {stats.avgSteps:F0} steps
        [green]🎯 Goal days:[/] {stats.goalDays:N0}
        [green]🏆 Best day:[/] {stats.bestDay}
        """);

  return new Panel(content)
      .Header("🚶‍♂️ Walking Statistics")
      .Border(BoxBorder.Rounded)
      .BorderColor(Color.Green);
}

void ShowCombinedInsights()
{
  Console.WriteLine();
  var rule = new Spectre.Console.Rule("[bold yellow]🧠 Health Insights[/]").RuleStyle("yellow");
  AnsiConsole.Write(rule);

  try
  {
    using (SqliteConnection connection = new SqliteConnection(connectionString))
    {
      connection.Open();

      string today = DateTime.Today.ToString("dd-MM-yy");

      using (SqliteCommand todayCmd = connection.CreateCommand())
      {
        todayCmd.CommandText = @"
                    SELECT 
                        COALESCE((SELECT SUM(Quantity) FROM drinking_water WHERE Date = $today), 0) as water,
                        COALESCE((SELECT SUM(Steps) FROM walking_activity WHERE Date = $today), 0) as steps
                ";
        todayCmd.Parameters.AddWithValue("$today", today);

        using (SqliteDataReader reader = todayCmd.ExecuteReader())
        {
          if (reader.Read())
          {
            int todayWater = reader.GetInt32("water");
            int todaySteps = reader.GetInt32("steps");

            Console.WriteLine();
            AnsiConsole.MarkupLine($"📅 Today's Progress:");
            AnsiConsole.MarkupLine($"   💧 Water: {todayWater}/8 glasses ({(double)todayWater / 8 * 100:F0}%)");
            AnsiConsole.MarkupLine($"   👣 Steps: {todaySteps:N0}/10,000 steps ({(double)todaySteps / 10000 * 100:F0}%)");

            if (todayWater >= 8 && todaySteps >= 10000)
            {
              AnsiConsole.MarkupLine("\n[green]🎉 Excellent! You've achieved both daily goals today![/]");
            }
            else if (todayWater >= 8)
            {
              AnsiConsole.MarkupLine("\n[cyan]💧 Great hydration today! Consider adding more steps to reach your walking goal.[/]");
            }
            else if (todaySteps >= 10000)
            {
              AnsiConsole.MarkupLine("\n[green]👣 Great walking today! Don't forget to stay hydrated.[/]");
            }
            else
            {
              AnsiConsole.MarkupLine("\n[yellow]💪 Keep going! You can still achieve your daily goals.[/]");
            }
          }
        }
      }
    }
  }
  catch (Exception ex)
  {
    AnsiConsole.MarkupLine($"[red]❌ Error calculating insights: {ex.Message}[/]");
  }
}

void ShowWeeklyCombinedChart()
{
  Console.WriteLine();
  var rule = new Spectre.Console.Rule("[bold blue]📈 Weekly Overview[/]").RuleStyle("blue");
  AnsiConsole.Write(rule);

  try
  {
    var table = new Table()
        .Border(TableBorder.Rounded)
        .BorderColor(Color.Blue)
        .AddColumn(new TableColumn("[bold]Date[/]").Centered())
        .AddColumn(new TableColumn("[bold]Water (glasses)[/]").Centered())
        .AddColumn(new TableColumn("[bold]Steps[/]").Centered())
        .AddColumn(new TableColumn("[bold]Goals Met[/]").Centered());

    using (SqliteConnection connection = new SqliteConnection(connectionString))
    {
      connection.Open();

      for (int i = 6; i >= 0; i--)
      {
        DateTime date = DateTime.Today.AddDays(-i);
        string dateStr = date.ToString("dd-MM-yy");

        using (SqliteCommand dayCmd = connection.CreateCommand())
        {
          dayCmd.CommandText = @"
                        SELECT 
                            COALESCE((SELECT SUM(Quantity) FROM drinking_water WHERE Date = $date), 0) as water,
                            COALESCE((SELECT SUM(Steps) FROM walking_activity WHERE Date = $date), 0) as steps
                    ";
          dayCmd.Parameters.AddWithValue("$date", dateStr);

          using (SqliteDataReader reader = dayCmd.ExecuteReader())
          {
            if (reader.Read())
            {
              int water = reader.GetInt32("water");
              int steps = reader.GetInt32("steps");

              string waterDisplay = water >= 8 ? $"[green]{water}[/] ✅" : $"[yellow]{water}[/]";
              string stepsDisplay = steps >= 10000 ? $"[green]{steps:N0}[/] ✅" : $"[yellow]{steps:N0}[/]";

              string goalsMetDisplay = "";
              if (water >= 8 && steps >= 10000)
                goalsMetDisplay = "[green]Both ✅✅[/]";
              else if (water >= 8)
                goalsMetDisplay = "[cyan]Water ✅[/]";
              else if (steps >= 10000)
                goalsMetDisplay = "[green]Steps ✅[/]";
              else
                goalsMetDisplay = "[dim]None[/]";

              table.AddRow(
                  date.ToString("MMM dd"),
                  waterDisplay,
                  stepsDisplay,
                  goalsMetDisplay
              );
            }
          }
        }
      }
    }

    AnsiConsole.Write(table);
  }
  catch (Exception ex)
  {
    AnsiConsole.MarkupLine($"[red]❌ Error creating weekly chart: {ex.Message}[/]");
  }
}

void CreateDatabase()
{
  using (SqliteConnection connection = new SqliteConnection(connectionString))
  {
    using (SqliteCommand tableCmd = connection.CreateCommand())
    {
      connection.Open();

      // Create water table
      tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS drinking_water (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Quantity INTEGER,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                )";
      tableCmd.ExecuteNonQuery();

      tableCmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS walking_activity (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Date TEXT,
                    Steps INTEGER,
                    Distance REAL,
                    Description TEXT,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
                )";
      tableCmd.ExecuteNonQuery();
    }
  }

}

void ClearConsoleAndWait(string message = "Press any key to continue...")
{
  AnsiConsole.MarkupLine($"\n[dim]{message}[/]");
  Console.ReadKey();
  Console.Clear();
}
void WarningMessage()
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine("Invalid choice. Please select on of the above and try again.\n");
  Console.ResetColor();
}

record WalkingRecord(
    int Id,
    DateTime Date,
    int Meters,
    string Description,
    DateTime CreatedAt
);


