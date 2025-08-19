namespace HabitLogger.JonesKwameOsei;

internal class Helpers
{
    internal void PrintMainHeader()
    {
        Console.Clear();

        // Create a gradient-like effect using different shades
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("██████████████████████████████████████████████████████████████████");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                              ██");
        Console.WriteLine("██          💧 H A B I T  💖  T R A C K E R 🚶               ██");
        Console.WriteLine("██                                                              ██");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("██          ▪▪▪ Your Health Journey Starts Here ▪▪▪           ██");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                              ██");

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("██████████████████████████████████████████████████████████████████");

        Console.WriteLine();

        HeaderTime();
    }

    internal void PrintWalkingTrackerHeader()
    {
        Console.Clear();

        // Create a gradient-like effect using different shades
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("██████████████████████████████████████████████████████████████████");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                              ██");
        Console.WriteLine("██          🚶‍ W A L K I N G  💖  T R A C K E R 🚶            ██");
        Console.WriteLine("██                                                              ██");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("██              ▪▪▪ Stay healthy by walking ▪▪▪               ██");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                              ██");

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("██████████████████████████████████████████████████████████████████");

        Console.WriteLine();

        HeaderTime();
    }

    internal void PrintWaterTrackerHeader()
    {
        Console.Clear();

        // Create a gradient-like effect using different shades
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("████████████████████████████████████████████████████████████████");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                            ██");
        Console.WriteLine("██           💧 H Y D R A T I O N   T R A C K E R 💧         ██");
        Console.WriteLine("██                                                            ██");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("██                ▪▪▪ Track your water intake ▪▪▪            ██");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("██                                                            ██");

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("████████████████████████████████████████████████████████████████");

        Console.WriteLine();

        HeaderTime();
    }

    private void HeaderTime()
    {
        // Modern info bar
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  ▶ Today: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(DateTime.Now.ToString("MMM dd, yyyy"));

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("    ▶ Time: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(DateTime.Now.ToString("HH:mm"));

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄");
        Console.WriteLine();

        Console.ResetColor();
    }
}
