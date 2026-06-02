using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using TodoList.Exceptions;

namespace TodoList
{
    public class FileManager : IDataStorage
    {
        private readonly string _dataDirectory;

        public FileManager(string dataDirectory)
        {
            _dataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
            EnsureDataDirectory();
        }

        private void EnsureDataDirectory()
        {
            if (!Directory.Exists(_dataDirectory))
                Directory.CreateDirectory(_dataDirectory);
        }

        private string GetProfilesPath() => Path.Combine(_dataDirectory, "profiles.csv");
        private string GetTodosPath(Guid userId) => Path.Combine(_dataDirectory, $"todos_{userId}.csv");

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            if (profiles == null) throw new ArgumentNullException(nameof(profiles));
            var path = GetProfilesPath();
            try
            {
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                using var buffered = new BufferedStream(fs);
                using var aes = CryptoConfig.CreateAes();
                using var crypto = new CryptoStream(buffered, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using var writer = new StreamWriter(crypto, Encoding.UTF8);
                foreach (var p in profiles)
                {
                    writer.WriteLine($"{p.Id};{EscapeCsv(p.Login)};{EscapeCsv(p.Password)};{EscapeCsv(p.FirstName)};{EscapeCsv(p.LastName)};{p.BirthYear}");
                }
                writer.Flush();
            }
            catch (IOException ex)
            {
                throw new DataStorageException($"Ошибка доступа к файлу профилей: {ex.Message}", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException($"Ошибка шифрования при сохранении профилей: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new DataStorageException($"Неожиданная ошибка при сохранении профилей: {ex.Message}", ex);
            }
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            var path = GetProfilesPath();
            if (!File.Exists(path))
                return new List<Profile>();

            var profiles = new List<Profile>();
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var buffered = new BufferedStream(fs);
                using var aes = CryptoConfig.CreateAes();
                using var crypto = new CryptoStream(buffered, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(crypto, Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = SplitCsvLine(line);
                    if (parts.Length != 6) continue;
                    if (!Guid.TryParse(parts[0], out var id)) continue;
                    var profile = new Profile(
                        id,
                        UnescapeCsv(parts[1]),
                        UnescapeCsv(parts[2]),
                        UnescapeCsv(parts[3]),
                        UnescapeCsv(parts[4]),
                        int.Parse(parts[5])
                    );
                    profiles.Add(profile);
                }
                return profiles;
            }
            catch (IOException ex)
            {
                throw new DataStorageException($"Ошибка доступа к файлу профилей: {ex.Message}", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException($"Ошибка расшифровки профилей: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new DataStorageException($"Неожиданная ошибка при загрузке профилей: {ex.Message}", ex);
            }
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user ID", nameof(userId));
            if (todos == null) throw new ArgumentNullException(nameof(todos));
            var path = GetTodosPath(userId);
            try
            {
                using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                using var buffered = new BufferedStream(fs);
                using var aes = CryptoConfig.CreateAes();
                using var crypto = new CryptoStream(buffered, aes.CreateEncryptor(), CryptoStreamMode.Write);
                using var writer = new StreamWriter(crypto, Encoding.UTF8);
                int index = 0;
                foreach (var item in todos)
                {
                    string text = EscapeCsv(item.Text);
                    string status = item.Status.ToString();
                    string lastUpdate = item.LastUpdate.ToString("o");
                    writer.WriteLine($"{index};{text};{status};{lastUpdate}");
                    index++;
                }
                writer.Flush();
            }
            catch (IOException ex)
            {
                throw new DataStorageException($"Ошибка доступа к файлу задач пользователя {userId}: {ex.Message}", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException($"Ошибка шифрования задач пользователя {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new DataStorageException($"Неожиданная ошибка при сохранении задач: {ex.Message}", ex);
            }
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            if (userId == Guid.Empty) throw new ArgumentException("Invalid user ID", nameof(userId));
            var path = GetTodosPath(userId);
            if (!File.Exists(path))
                return new List<TodoItem>();

            var todos = new List<TodoItem>();
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var buffered = new BufferedStream(fs);
                using var aes = CryptoConfig.CreateAes();
                using var crypto = new CryptoStream(buffered, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var reader = new StreamReader(crypto, Encoding.UTF8);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = SplitCsvLine(line);
                    if (parts.Length != 4) continue;
                    string text = UnescapeCsv(parts[1]);
                    if (!Enum.TryParse<TodoStatus>(parts[2], true, out var status)) continue;
                    if (!DateTime.TryParse(parts[3], null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastUpdate)) continue;
                    todos.Add(new TodoItem(text, status, lastUpdate));
                }
                return todos;
            }
            catch (IOException ex)
            {
                throw new DataStorageException($"Ошибка доступа к файлу задач пользователя {userId}: {ex.Message}", ex);
            }
            catch (CryptographicException ex)
            {
                throw new DataStorageException($"Ошибка расшифровки задач пользователя {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new DataStorageException($"Неожиданная ошибка при загрузке задач: {ex.Message}", ex);
            }
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";
            text = text.Replace("\"", "\"\"");
            text = text.Replace("\n", "\\n");
            return $"\"{text}\"";
        }

        private static string UnescapeCsv(string text)
        {
            if (text.Length >= 2 && text.StartsWith("\"") && text.EndsWith("\""))
                text = text.Substring(1, text.Length - 2);
            text = text.Replace("\\n", "\n");
            text = text.Replace("\"\"", "\"");
            return text;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == ';' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}