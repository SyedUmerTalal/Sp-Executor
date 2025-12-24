using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace SpExecutor
{
    class Program
    {
        static bool keepRunning = true;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\nStopping Sp Executor...");
                keepRunning = false;
                e.Cancel = true;
            };

            try
            {
                string configPath = "config.txt";
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Config file not found.");
                    return;
                }

                string dbIP = "", dbName = "", dbUser = "", dbPassword = "";
                int delaySeconds = 0;
                List<string> storedProcedures = new List<string>();
                bool spSection = false;

                foreach (var line in File.ReadAllLines(configPath))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("DatabaseIP="))
                        dbIP = line.Split('=')[1].Trim();
                    else if (line.StartsWith("DatabaseName="))
                        dbName = line.Split('=')[1].Trim();
                    else if (line.StartsWith("DbUser="))
                        dbUser = line.Split('=')[1].Trim();
                    else if (line.StartsWith("DbPassword="))
                        dbPassword = line.Split('=')[1].Trim();
                    else if (line.StartsWith("DelaySeconds="))
                        delaySeconds = int.Parse(line.Split('=')[1].Trim());
                    else if (line.StartsWith("StoredProcedures:"))
                        spSection = true;
                    else if (spSection)
                        storedProcedures.Add(line.Trim());
                }

                string connStr =
                    $"Data Source={dbIP};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};";

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("Catalyst IT Solutions");
                    Console.ResetColor();
                    Console.WriteLine("Database Connected.");
                    Console.WriteLine("Sp Executor started. Press CTRL+C to stop.\n");

                    while (keepRunning)
                    {
                        foreach (var sp in storedProcedures)
                        {
                            if (!keepRunning)
                                break;

                            try
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"Running SP: {sp}");

                                using (SqlCommand cmd = new SqlCommand(sp, conn))
                                {
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.CommandTimeout = 0;
                                    cmd.ExecuteNonQuery();
                                }
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Completed: {sp}");
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Error in SP: {sp}");
                                Console.WriteLine(ex.Message);
                                Console.ResetColor();
                            }
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Waiting {delaySeconds} seconds before next SP...\n");
                            Console.ResetColor();

                            Thread.Sleep(delaySeconds * 1000);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.Message);
            }

            Console.WriteLine("Sp Executor stopped.");
        }
    }

}
