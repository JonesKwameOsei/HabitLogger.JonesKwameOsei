using HabitLogger.JonesKwameOsei;
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Data;
using System.Globalization;
using System.Windows.Markup;
using static System.Runtime.InteropServices.JavaScript.JSType;

internal class WalkTracker
{
    private const int DAILY_GOAL_STEPS = 10000;
    private const double DAILY_GOAL_KM = 8.0;
    private readonly string connectionString;
    private readonly UtilityHelpers _utilityHelpers = new UtilityHelpers();
    private readonly SharedHelpers _sharedHelpers = new SharedHelpers();

    public WalkTracker(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public void AddRecord()
    {
        Console.Clear();

        Panel panel = new Panel(new Text("🚶‍ LOG WALKING ACTIVITY 🚶", new Style(Color.Green, Color.Default, Decoration.Bold)))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);
        AnsiConsole.Write(panel);
        Console.WriteLine();

        try
        {
            string date = _sharedHelpers.GetDate("\n📅 Enter the date (format: dd-MM-yy) or insert insert [red]0[/] to Return to Main Menu: ");
            if (date == "0") return;

            string trackingChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\n📏 How would you like to track your walking activity?")
                    .AddChoices("Steps", "Distance (KM)", "Both")
            );

            int steps = 0;
            double distance = 0;

            switch (trackingChoice)
            {
                case "Steps":
                    steps = GetSteps("👣 Enter number of steps walked (or [red]0[/] to return to menu)");
                    if (steps == 0) return;
                    distance = StepsToKilometres(steps);
                    break;

                case "Distance (KM)":
                    distance = GetDistance("🚶‍ Enter distance walked in kilometers (or [red]0[/] to return to menu)");
                    if (distance == 0) return;
                    steps = KilometresToSteps(distance);
                    break;

                case "Both":
                    steps = GetSteps("👣 Enter number of steps walked");
                    distance = GetDistance("🚶‍ Enter distance walked in kilometres");
                    break;
            }

            Console.Clear();
            string description = AnsiConsole.Ask<string>("📝 Enter a brief [green]description[/] of your walk (optional):");

            if (steps <= 0 || distance <= 0)
            {
                AnsiConsole.MarkupLine("[red]❌ Invalid data: Steps and distance must be greater than zero.[/]");
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey();
                return;
            }

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqliteCommand command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO walking_activity (Date, Steps, Distance, Description, CreatedAt)
                            VALUES ($date, $steps, $distance, $description, $createdAt)";

                        command.Parameters.AddWithValue("$date", DateTime.ParseExact(date, "dd-MM-yy", CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("$steps", steps);
                        command.Parameters.AddWithValue("$distance", distance);
                        command.Parameters.AddWithValue("$description", string.IsNullOrWhiteSpace(description) ? DBNull.Value : description);
                        command.Parameters.AddWithValue("$createdAt", DateTime.Now);

                        command.ExecuteNonQuery();
                        AnsiConsole.MarkupLine($"\n[green]✅ Successfully logged {steps:N0} steps ({distance:F2} km) on {date}![/]");
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
            AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred while adding your walking record:[/]");
            AnsiConsole.MarkupLine($"[red]   {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[yellow]⚠️  Please try again or restart the application.[/]");
        }
        finally
        {
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey();
            Console.Clear();
        }
    }

    public void ViewRecord()
    {
        Console.Clear();

        Panel panel = new Panel(new Text("🚶‍ VIEW WALKING RECORDS 🚶", new Style(Color.Green, Color.Default, Decoration.Bold)))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);
        AnsiConsole.Write(panel);
        Console.WriteLine();

        try
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                using (SqliteCommand selectCmd = connection.CreateCommand())
                {
                    selectCmd.CommandText = "SELECT * from walking_activity ORDER BY Date DESC, CreatedAt DESC";
                    using (SqliteDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            AnsiConsole.MarkupLine("\n[red]No records found. Start logging your walking activity![/]");
                            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                            Console.ReadKey();
                            Console.Clear();
                            return;
                        }

                        Table table = new Table()
                            .Border(TableBorder.Rounded)
                            .BorderColor(Color.Green)
                            .Title(new TableTitle("Walking Records"))
                            .AddColumn(new TableColumn("[bold]ID[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Date[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Steps[/]").RightAligned())
                            .AddColumn(new TableColumn("[bold]Distance (KM)[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Description[/]"))
                            .AddColumn(new TableColumn("[bold]Created At[/]"));

                        while (reader.Read())
                        {
                            int steps = reader.GetInt32("Steps");
                            double distance = reader.GetDouble("Distance");

                            string stepsDisplay = $"[green]{steps:N0}[/] 👣";
                            string distanceDisplay = $"[blue]{distance:F2}[/] 🚶";
                            string description = reader.IsDBNull("Description") ? "[dim]No description[/]" : reader.GetString("Description");

                            stepsDisplay = steps >= DAILY_GOAL_STEPS ? $"[bold green]{steps:N0}[/] 🎯" : $"[green]{steps:N0}[/] 👣";

                            table.AddRow(
                                reader.GetInt32("Id").ToString(),
                                reader.GetString("Date"),
                                stepsDisplay,
                                distanceDisplay,
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

        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }


    public void DeleteRecord()
    {
        try
        {
            Console.Clear();
            Panel panel = new Panel(new Text("🗑️ DELETE WALKING RECORD", new Style(Color.Red, Color.Default, Decoration.Bold)))
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
            }

            int recordId = AnsiConsole.Ask<int>("Enter the [red]ID[/] of the record you want to delete (or [red]0[/] to cancel):");

            if (recordId == 0) return;

            if (recordId < 0)
            {
                _utilityHelpers.DBInputErrorMessage();
            }

            if (!AnsiConsole.Confirm($"Are you sure you want to delete record ID {recordId}?"))
                return;

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (SqliteCommand deleteCmd = connection.CreateCommand())
                {
                    deleteCmd.CommandText = "DELETE FROM walking_activity WHERE Id = $id";
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
        }
        catch (Exception ex)
        {
            _utilityHelpers.HandleSpecialException(ex);
        }

        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    public void UpdateRecord()
    {
        try
        {
            Console.Clear();
            Panel panel = new Panel(new Text("✏️  UPDATE WALKING RECORD", new Style(Color.Yellow, Color.Black, Decoration.Bold)))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Yellow);
            AnsiConsole.Write(panel);

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
                recordId = AnsiConsole.Ask<int>("🪪 Enter the [cyan]ID[/] of the record you want to update (or [red]0[/] to cancel):");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Invalid input: {ex.Message}[/]");
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey();
                return;
            }

            if (recordId == 0) return;

            try
            {
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    string currentDate = "";
                    int currentSteps = 0;
                    double currentDistance = 0.0;
                    string currentDescription = "";

                    using (SqliteCommand selectCmd = connection.CreateCommand())
                    {
                        selectCmd.CommandText = "SELECT * FROM walking_activity WHERE Id = $id";
                        selectCmd.Parameters.AddWithValue("$id", recordId);

                        using (SqliteDataReader reader = selectCmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                AnsiConsole.MarkupLine("[red]❌ Record not found![/]");
                                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                                Console.ReadKey();
                                return;
                            }

                            // Safely extract data with null checks
                            currentDate = reader.IsDBNull("Date") ? DateTime.Today.ToString("dd-MM-yy") : reader.GetString("Date");
                            currentSteps = reader.IsDBNull("Steps") ? 0 : reader.GetInt32("Steps");
                            currentDistance = reader.IsDBNull("Distance") ? 0.0 : reader.GetDouble("Distance");
                            currentDescription = reader.IsDBNull("Description") ? "" : reader.GetString("Description");
                        }
                    }

                    Console.WriteLine();
                    AnsiConsole.MarkupLine($"[dim]Current Values:[/]");
                    AnsiConsole.MarkupLine($"Date: [cyan]{currentDate}[/]");
                    AnsiConsole.MarkupLine($"Steps: [cyan]{currentSteps:N0}[/]");
                    AnsiConsole.MarkupLine($"Distance: [cyan]{currentDistance:F2} km[/]");
                    AnsiConsole.MarkupLine($"Description: [cyan]{(string.IsNullOrEmpty(currentDescription) ? "No description" : currentDescription)}[/]");
                    Console.WriteLine();

                    string newDate;
                    int newSteps;
                    double newDistance;
                    string newDescription;

                    try
                    {
                        newDate = _sharedHelpers.GetValidDate($"Enter new date (current: {currentDate}):", currentDate);
                        newSteps = _sharedHelpers.GetValidInteger($"Enter new number of steps (current: {currentSteps:N0}):", currentSteps, 0, 100000, "Steps");
                        newDistance = _sharedHelpers.GetValidDouble($"Enter new distance in KM (current: {currentDistance:F2}):", currentDistance, 0, 200, "Distance");
                        newDescription = AnsiConsole.Ask($"Enter new description (current: {(string.IsNullOrEmpty(currentDescription) ? "No description" : currentDescription)}):", currentDescription);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]❌ Input error: {ex.Message}[/]");
                        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                        Console.ReadKey();
                        return;
                    }

                    using (SqliteCommand updateCmd = connection.CreateCommand())
                    {
                        updateCmd.CommandText = @"
                        UPDATE walking_activity
                        SET Date = $date, Steps = $steps, Distance = $distance, Description = $description 
                        WHERE Id = $id";
                        updateCmd.Parameters.AddWithValue("$date", newDate);
                        updateCmd.Parameters.AddWithValue("$steps", newSteps);
                        updateCmd.Parameters.AddWithValue("$distance", newDistance);
                        updateCmd.Parameters.AddWithValue("$description", newDescription);
                        updateCmd.Parameters.AddWithValue("$id", recordId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            AnsiConsole.MarkupLine("[green]✅ Record updated successfully![/]");
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("[yellow]⚠️ No rows were updated. Record may have been deleted.[/]");
                        }
                    }
                }
            }
            catch (SqliteException sqlEx)
            {
                _utilityHelpers.DBUpdateErrorMessage(sqlEx);
                return;
            }
            catch (InvalidOperationException ioEx)
            {
                _utilityHelpers.DBOperationErrorMessage(ioEx);
                return;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred: {ex.Message}[/]");
                AnsiConsole.MarkupLine($"[dim]Error Type: {ex.GetType().Name}[/]");
                AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
                Console.ReadKey();
                return;
            }
        }
        catch (Exception globalEx)
        {
            AnsiConsole.MarkupLine($"[red]❌ Critical error in UpdateRecord: {globalEx.Message}[/]");
            AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
            Console.ReadKey();
            return;
        }

        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
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
                    selectCmd.CommandText = "SELECT SUM(Steps), SUM(Distance) FROM walking_activity WHERE Date = $date";
                    selectCmd.Parameters.AddWithValue("$date", date);
                    using (SqliteDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int totalSteps = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            double totalDistance = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);

                            AnsiConsole.MarkupLine($"\n[green]Today's Progress for {date}:[/]");
                            AnsiConsole.MarkupLine($"[bold]Total Steps:[/] [green]{totalSteps:N0}[/] 👣");
                            AnsiConsole.MarkupLine($"[bold]Total Distance:[/] [blue]{totalDistance:F2}[/] 🚶");
                            if (totalSteps >= DAILY_GOAL_STEPS)
                            {
                                AnsiConsole.MarkupLine("[bold green]🎯 Congratulations! You've reached your daily step goal![/]");
                            }
                            else
                            {
                                AnsiConsole.MarkupLine($"[yellow]Keep going! You need [bold]{DAILY_GOAL_STEPS - totalSteps:N0}[/] more steps to reach your daily goal.[/]");
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

    private void ShowWeeklyProgress()
    {
        Console.WriteLine();

        var rule = new Spectre.Console.Rule("[yellow]📈 Last 7 Days Walking Progress[/]").RuleStyle("yellow");
        AnsiConsole.Write(rule);

        BarChart chart = new BarChart()
            .Width(60)
            .Label("[green bold underline]Weekly Steps Progress[/]")
            .CenterLabel()
            .AddItem("Steps", 0, Color.Green)
            .AddItem("Distance", 0, Color.Blue);

        try
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = DateTime.Today.AddDays(-i);
                    string dateStr = date.ToString("dd-MM-yy");

                    using (SqliteCommand selectCmd = connection.CreateCommand())
                    {
                        selectCmd.CommandText = "SELECT SUM(Steps) FROM walking_activity WHERE Date = $date";
                        selectCmd.Parameters.AddWithValue("$date", dateStr);

                        object? result = selectCmd.ExecuteScalar();
                        int totalSteps = result != DBNull.Value ? Convert.ToInt32(result) : 0;

                        Color barColor = totalSteps >= DAILY_GOAL_STEPS ? Color.Green : Color.Orange1;
                        chart.AddItem(date.ToString("MMM dd"), totalSteps, barColor);
                    }
                }
                AnsiConsole.Write(chart);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Error retrieving weekly progress: {ex.Message}[/]");
        }
    }

    private void DisplayProgressBar(int steps, double distance)
    {
        try
        {
            if (steps < 0 || distance < 0)
            {
                AnsiConsole.MarkupLine("[red]❌ Invalid input: Steps and distance cannot be negative.[/]");
                return;
            }

            double stepPercentage = Math.Min((double)steps / DAILY_GOAL_STEPS, 1.0);
            double distancePercentage = Math.Min(distance / DAILY_GOAL_KM, 1.0);

            var rule = new Spectre.Console.Rule("[green]📊 Daily Progress[/]").RuleStyle("green");
            AnsiConsole.Write(rule);

            Console.WriteLine();
            AnsiConsole.MarkupLine($"👣 Steps: [green]{steps:N0}[/] / [dim]{DAILY_GOAL_STEPS:N0}[/] ({stepPercentage:P0})");

            string stepsProgressBar = CreateProgressBar(0.5, 30, '█', '░');
            Color stepsBorderColor = stepPercentage >= 1.0 ? Color.Green : Color.Yellow;
            var stepsPanel = CreateProgressPanel(stepsProgressBar, "Steps Progress", stepsBorderColor, "green");
            AnsiConsole.Write(stepsPanel);

            Console.WriteLine();

            AnsiConsole.MarkupLine($"🚶 Distance: [cyan]{distance:F2} km[/] / [dim]{DAILY_GOAL_KM:F1} km[/] ({distancePercentage:P0})");

            string distanceProgressBar = _sharedHelpers.CreateProgressBar(distancePercentage, width: 30, fillChar: '█', emptyChar: '░');
            Color distanceBorderColor = distancePercentage >= 1.0 ? Color.Cyan1 : Color.Yellow;
            var distancePanel = _sharedHelpers.CreateProgressPanel(distanceProgressBar, "Distance Progress", distanceBorderColor, "cyan");
            AnsiConsole.Write(distancePanel);

            string statusMessage = (stepPercentage, distancePercentage) switch
            {
                ( >= 1.0, >= 1.0) => "[green]🎉 Amazing! You've exceeded both daily goals![/]",
                ( >= 1.0, _) => "[green]🎯 Great! You've reached your step goal![/]",
                (_, >= 1.0) => "[green]🏃‍♂️ Excellent! You've reached your distance goal![/]",
                ( >= 0.75, _) => "[yellow]💪 Almost there! Keep walking![/]",
                ( >= 0.5, _) => "[cyan]👍 Good progress! Halfway to your goal![/]",
                _ => "[magenta]🚀 Great start! Every step counts![/]"
            };

            Console.WriteLine();
            AnsiConsole.MarkupLine(statusMessage);

            Console.WriteLine();

            try
            {
                double calories = CalculateCalories(steps);
                AnsiConsole.MarkupLine($"🔥 Estimated calories burned: [orange1]{calories:F0}[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]⚠️ Could not calculate calories: {ex.Message}[/]");
            }

            try
            {
                double walkingTime = CalculateWalkingTime(distance);
                AnsiConsole.MarkupLine($"⏱️ Estimated walking time: [blue]{walkingTime:F0} minutes[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[yellow]⚠️ Could not calculate walking time: {ex.Message}[/]");
            }
        }
        catch (DivideByZeroException)
        {
            AnsiConsole.MarkupLine("[red]❌ Error: Invalid daily goal values (cannot be zero).[/]");
        }
        catch (OverflowException ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ Calculation overflow error: {ex.Message}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]❌ An unexpected error occurred while displaying progress: {ex.Message}[/]");
        }
    }

    private double CalculateCalories(int steps)
    {
        const double caloriesPerStep = 0.04;
        return steps * caloriesPerStep;
    }

    private double CalculateWalkingTime(double distanceKm)
    {
        const double averageWalkingSpeedKmPerHour = 5.0;
        if (averageWalkingSpeedKmPerHour <= 0)
            throw new ArgumentException("Walking speed must be greater than zero");

        return (distanceKm / averageWalkingSpeedKmPerHour) * 60;
    }


    private void ShowRecordsForSelection()
    {
        string[] columns = { "Id", "Date", "Steps", "Distance", "Description" };
        _sharedHelpers.ShowRecordsForSelection(connectionString, "walking_activity", columns, "Walking Records", Color.DarkSlateGray1);
    }

    public void ShowStatistics()
    {
        Console.Clear();

        Panel panel = new Panel(new Text("📊 WALKING STATISTICS 📊", new Style(Color.Plum4, Color.Default, Decoration.Bold)))
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);
        AnsiConsole.Write(panel);

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();

            using (SqliteCommand totalCmd = connection.CreateCommand())
            {
                totalCmd.CommandText = "SELECT COUNT(*), SUM(Steps), SUM(Distance) FROM walking_activity";
                using (SqliteDataReader reader = totalCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int totalEntries = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        int totalSteps = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                        double totalDistance = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                        Console.WriteLine();
                        AnsiConsole.MarkupLine($"📋 Total entries: [cyan]{totalEntries}[/]");
                        AnsiConsole.MarkupLine($"👣 Total steps: [green]{totalSteps:N0}[/]");
                        AnsiConsole.MarkupLine($"🚶‍ Total distance: [cyan]{totalDistance:F2} KM[/]");
                        AnsiConsole.MarkupLine($"🔥 Total calories burned: [orange4]{totalDistance:F2} KM[/]");

                        if (totalEntries > 0)
                        {
                            AnsiConsole.MarkupLine($"📈 Average steps/day: [yellow]{totalSteps / totalEntries:F0}[/]");
                            AnsiConsole.MarkupLine($"📈 Average distance/day: [yellow]{totalDistance / totalEntries:F2} km[/]");
                        }
                    }
                }
            }

            using (SqliteCommand goalCmd = connection.CreateCommand())
            {
                goalCmd.CommandText = @"
                    SELECT COUNT(*) FROM (
                        SELECT Date
                        FROM walking_activity
                        GROUP BY Date
                        HAVING SUM(Steps) >= $goal
                    )";
                goalCmd.Parameters.AddWithValue("$goal", DAILY_GOAL_STEPS);

                object? result = goalCmd.ExecuteScalar();
                int goalDays = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                AnsiConsole.MarkupLine($"🎯 Days steps goal achieved: [green]{goalDays}[/]");
            }

            using (SqliteCommand bestDayCmd = connection.CreateCommand())
            {
                bestDayCmd.CommandText = @"
                   SELECT Date, SUM(Steps) as TotalSteps, SUM(Distance) as TotalDistance
                    FROM walking_activity
                    GROUP BY Date
                    ORDER BY TotalSteps DESC
                    LIMIT 1";

                using (SqliteDataReader reader = bestDayCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string bestDate = reader.GetString("Date");
                        int bestSteps = reader.GetInt32("TotalSteps");
                        double bestDistance = reader.GetDouble("TotalDistance");
                        AnsiConsole.MarkupLine($"🏆 Best day: [gold3]{bestDate}[/] - [gold3]{bestSteps:N0} steps ({bestDistance:F2} KM)[/]");
                    }
                }
            }
        }

        if (_sharedHelpers.PromptForWeeklyProgressReport())
        {
            ShowWeeklyProgress();
        }
        AnsiConsole.MarkupLine("\n[dim]Press any key to continue...[/]");
        Console.ReadKey();
    }

    private string CreateProgressBar(double percentage, int width = 30, char fillChar = '█', char emptyChar = '░')
    {
        percentage = Math.Clamp(percentage, 0.0, 1.0);
        int filledBars = (int)(percentage * width);
        return "[" + new string(fillChar, filledBars).PadRight(width, emptyChar) + "]";
    }

    private Panel CreateProgressPanel(string progressBar, string header, Color borderColor, string textColor)
    {
        return new Panel(new Markup($"[{textColor}]{progressBar}[/]"))
            .Header(header)
            .Border(BoxBorder.Rounded)
            .BorderColor(borderColor)
            .Expand();
    }

    private int GetSteps(string message)
    {
        return AnsiConsole.Ask<int>($"[green]{message}[/]");
    }

    private double GetDistance(string message)
    {
        return AnsiConsole.Ask<double>($"[green]{message}[/]");
    }

    private string GetDate(string message)
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

    private double StepsToKilometres(int steps)
    {
        const double averageStepLength = 0.000762;
        return steps * averageStepLength;
    }

    private int KilometresToSteps(double kilometres)
    {
        const double averageStepLength = 0.000762;
        return (int)(kilometres / averageStepLength);
    }

}
