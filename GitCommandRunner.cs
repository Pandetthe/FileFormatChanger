using System;
using System.Diagnostics;

namespace FileFormatChanger
{
    public class GitCommandRunner
    {
        // Publiczna zmienna miejsca wywoływania poleceń git
        public string WorkingDirectory { get; }

        public GitCommandRunner(string workingDirectory)
        {
            WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public string Run(string arguments)
        {
            // Ustawianie odpowiednich wartości, potrzebnych do uruchomienia procesu
            ProcessStartInfo info = new ProcessStartInfo("git", arguments)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = WorkingDirectory,
            };
            // Tworzenie nowego procesu
            Process process = new Process()
            {
                // Przypisywanie wartości startowych
                StartInfo = info,
            };
            // Startowanie procesu
            process.Start();

            // Zwracanie informacji zwrotnej polecenia git
            return process.StandardOutput.ReadToEnd();
        }
    }
}