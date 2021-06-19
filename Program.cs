using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;

namespace FileFormatChanger
{
    public class Program
    {
        static void Main(string[] args)
        {
            string dir1, dir2, var1, var2, commitName; // Deklarowanie zmiennych
            string sDir1 = ReadSetting("dir1"), sDir2 = ReadSetting("dir2"),
                sVar1 = ReadSetting("var1"), sVar2 = ReadSetting("var2"); // Wczytywanie ostatnich zapisanych ustawień
            byte[] data;

            // Pozyskiwanie odpowiednich danych
            dir2 = @GetDirectory("Głównego folderu projektu", sDir2);
            dir1 = @GetDirectory("Operacji", sDir1);
            var1 = @GetFile(dir1, "Plik bazowy 1", sVar1);
            var2 = @GetFile(dir1, "Plik bazowy 2", sVar2);

            // Sprawdzanie czy plik var1 oraz var2 nie są takie same
            while (var1 == var2)
            {
                Console.WriteLine("Plik bazowy 1 jest taki sam jak plik bazowy 2");
                var2 = @GetFile(dir1, "Plik bazowy 2", sVar2);
            }

            // Usuwanie odmian pliku bazowego 1
            File.Delete(Path.Combine(dir1, string.Format(@"{0}Linux.data", var1)));
            File.Delete(Path.Combine(dir1, string.Format(@"{0}Windows.data", var1)));

            // Usuwanie odmian pliku bazowego 2
            File.Delete(Path.Combine(dir1, string.Format(@"{0}Linux.data", var2)));
            File.Delete(Path.Combine(dir1, string.Format(@"{0}Windows.data", var2)));

            // Tworzenie odmiany Windowsowej pliku bazowego 1
            File.Copy(Path.Combine(dir1, string.Format("{0}.data", var1)),
                Path.Combine(dir1, string.Format("{0}Windows.data", var1)));

            // Zczytywanie bajtów pliku bazowego 1
            data = File.ReadAllBytes(Path.Combine(dir1, string.Format("{0}.data", var1)));
            
            // Tworzenie odmiany Linuxowej pliku bazowego 1
            WriteLinuxFile(Path.Combine(dir1, string.Format("{0}Linux.data", var1)), data);

            // Tworzenie odmiany Windowsowej pliku bazowego 1
            File.Copy(Path.Combine(dir1, string.Format("{0}.data", var2)),
                Path.Combine(dir1, string.Format("{0}Windows.data", var2)));

            // Zczytywanie bajtów pliku bazowego 1
            data = File.ReadAllBytes(Path.Combine(dir1, string.Format("{0}.data", var2)));

            // Tworzenie odmiany Linuxowej pliku bazowego 1
            WriteLinuxFile(Path.Combine(dir1, string.Format("{0}Linux.data", var2)), data);
            
            // Usuwanie plików bazowych
            File.Delete(Path.Combine(dir1, string.Format(@"{0}.data", var1)));
            File.Delete(Path.Combine(dir1, string.Format(@"{0}.data", var2)));

            // Pozyskiwanie nazwy commita
            Console.Write("Podaj nazwę commita: ");
            commitName = Console.ReadLine();

            // Tworzenie nowej instancji GitCommandRunnera
            GitCommandRunner runner = new GitCommandRunner(dir2);

            // Wywoływanie poleceń git za pomocą GitCommandRunnera
            Console.Write(runner.Run("add ."));
            Console.Write(runner.Run($"commit -m \"{commitName}\""));
            Console.Write(runner.Run("push"));

            // Zapisywanie ustawień
            UpdateSettings(dir1, dir2, var1, var2);

            // Pozostawienie programu w trybie idle, aż do momentu naciśnięcia przycisku na klawiaturze
            Console.WriteLine("Naciśnij przycisk, aby zamknąć konsole . . .");
            Console.ReadKey();
        }

        static string GetDirectory(string name, string setting)
        {
            // Deklaracja zmiennych
            string dir = null, lastOption = setting != null ? "(" + setting + ")" : "";
            int b = 1;
            // Pętla zapętlająca się, do momentu gdy podana ścieżka plików będzie istniała. W momencie, gdy
            // ścieżka plików nie istnieje int b zostaje zwiększony o 1
            for (int a = 0; a < b; a++)
            {
                Console.Write($"Podaj ścieżkę plików \"{name}\"{lastOption}: ");
                dir = @Console.ReadLine();
                if (string.IsNullOrEmpty(dir) && setting != null)
                {
                    // Sprawdzanie czy taka ścieżka plików istnieje
                    if (Directory.Exists(setting))
                    {
                        // Jeżeli istnieje, przypisujemy wartości zwracanej wartość ustawienia
                        dir = setting;
                        continue;
                    }
                    Console.WriteLine("Ostatnia użyta ścieżka plików jest błędna!");
                    // Dodanie kolejnego loopa
                    b++;
                    continue;
                }
                // Sprawdzanie czy taka ścieżka plików istnieje
                if (Directory.Exists(dir))
                    continue;
                Console.WriteLine("Podałeś błędną ścieżke plików!");
                // Dodanie kolejnego loopa
                b++;
            }
            return dir;
        }

        static string GetFile(string dir, string name, string setting)
        {
            // Deklaracja zmiennych
            string file = null, lastOption = setting != null ? "(" + setting + ")" : "";
            string[] files;
            int b = 1;
            // Pętla zapętlająca się, do momentu gdy podany plik będzie istniał. W momencie, gdy plik nie istnieje
            // int b zostaje zwiększony o 1
            for (int a = 0; a < b; a++)
            {
                Console.Write($"Podaj nazwę pliku \"{name}\"{lastOption}: ");
                file = @Console.ReadLine();
                if (string.IsNullOrEmpty(file) && setting != null)
                {
                    // Sprawdzanie czy taki plik istnieje
                    files = Directory.GetFiles(dir, string.Format("{0}.data", setting));
                    if (files.Length != 0)
                    {
                        // Jeżeli istnieje, przypisujemy wartości zwracanej wartość ustawienia
                        file = setting;
                        continue;
                    }
                    Console.WriteLine("Ostatnia użyta nazwa pliku jest błędna!");
                    // Dodanie kolejnego loopa
                    b++;
                    continue;
                }
                // Sprawdzanie czy taki plik istnieje
                files = Directory.GetFiles(dir, string.Format("{0}.data", file));
                if (files.Length != 0)
                    continue;
                Console.WriteLine("Podałeś błędną nazwe pliku!");
                // Dodanie kolejnego loopa
                b++;
            }
            return file;
        }

        static string ReadSetting(string key)
        {
            try
            {
                // Kolekcja zapisanych ustawień aplikacji
                NameValueCollection appSettings = ConfigurationManager.AppSettings;

                // Jeżeli ustawienie o takim kluczu jest równy "" lub null zwracamy null
                if (string.IsNullOrEmpty(appSettings[key]))
                    return null;
                return appSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                return null;
            }
        }

        static void UpdateSettings(string dir1, string dir2, string var1, string var2)
        {
            // Otwarcie pliku konfiguracyjnego
            Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Zapisane wartości ustawień
            KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;

            // Przypisywanie wartości ustawień lub tworzenie nowych ustawień, jeżeli takowe nie istnieją
            if (settings["dir1"] == null)
                settings.Add("dir1", dir1);
            else
                settings["dir1"].Value = dir1;
            if (settings["dir2"] == null)
                settings.Add("dir2", dir2);
            else
                settings["dir2"].Value = dir2;
            if (settings["var1"] == null)
                settings.Add("var1", var1);
            else
                settings["var1"].Value = var1;
            if (settings["var2"] == null)
                settings.Add("var2", var2);
            else
                settings["var2"].Value = var2;

            // Zapisanie ustawień
            configFile.Save(ConfigurationSaveMode.Modified);
            // Odświeżenie sekcji konfiguracji
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        static void WriteLinuxFile(string file, byte[] data)
        {
            // Konwersja enkodowania z ANSI do UTF-8
            byte[] convertedData = Encoding.Convert(Encoding.Default, Encoding.UTF8, data);

            // Wartość HEX dla CRLF
            const byte CR = 0x0D, LF = 0x0A;
            using (FileStream fileStream = File.OpenWrite(file))
            {
                // Tworzenie nowej instancji
                BinaryWriter bw = new BinaryWriter(fileStream);

                int position = 0, index = 0;
                do
                {
                    // Przypisywanie zmiennej indeksu najbliższego dosowego EOL
                    index = Array.IndexOf(convertedData, CR, position);
                    if ((index >= 0) && (convertedData[index + 1] == LF))
                    {
                        // Zapisywanie bajtów z ominięciem bajta z zawartością CR
                        bw.Write(convertedData, position, index - position);
                        position = index + 1;
                    }
                }
                while (index >= 0);
                bw.Write(convertedData, position, convertedData.Length - position);
                // Zmiana długości pliku
                fileStream.SetLength(fileStream.Position);
            }
        }
    }
}
