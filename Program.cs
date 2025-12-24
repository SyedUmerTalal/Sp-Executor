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
        static void Main(string[] args)
        {
            string configPath = "config.txt";

            try
            {
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("Config file not found.");
                    return;
                }

                string dbIP = "";
                string dbName = "";
                string dbUser = "";
                string dbPassword = "";
                int delaySeconds = 0;

                List<string> storedProcedures = new List<string>();
                bool spSection = false;

                foreach (var line in File.ReadAllLines(configPath))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

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

                string connectionString =
                    $"Data Source={dbIP};Initial Catalog={dbName};User ID={dbUser};Password={dbPassword};";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine("Database connected successfully.\n");

                    foreach (var sp in storedProcedures)
                    {
                        try
                        {
                            Console.WriteLine($"Running SP : {sp}");

                            using (SqlCommand cmd = new SqlCommand(sp, conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandTimeout = 0;
                                cmd.ExecuteNonQuery();
                            }

                            Console.WriteLine($"Completed SP : {sp}");
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error in SP : {sp}");
                            Console.WriteLine(ex.Message);
                            Console.ResetColor();
                        }

                        Console.WriteLine($"Waiting {delaySeconds} seconds...\n");
                        Thread.Sleep(delaySeconds * 1000);
                    }
                }

                Console.WriteLine("Process completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal Error: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

}
