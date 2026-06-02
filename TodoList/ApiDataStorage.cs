using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using TodoList.Exceptions;

namespace TodoList
{
    public class ApiDataStorage : IDataStorage
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "http://localhost:5000/";

        public ApiDataStorage()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public void SaveProfiles(IEnumerable<Profile> profiles)
        {
            var task = Task.Run(() => SaveProfilesAsync(profiles));
            task.Wait();
        }

        public IEnumerable<Profile> LoadProfiles()
        {
            var task = Task.Run(() => LoadProfilesAsync());
            return task.Result;
        }

        public void SaveTodos(Guid userId, IEnumerable<TodoItem> todos)
        {
            var task = Task.Run(() => SaveTodosAsync(userId, todos));
            task.Wait();
        }

        public IEnumerable<TodoItem> LoadTodos(Guid userId)
        {
            var task = Task.Run(() => LoadTodosAsync(userId));
            return task.Result;
        }

        private async Task SaveProfilesAsync(IEnumerable<Profile> profiles)
        {
            string json = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = false });
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] encrypted = await EncryptAsync(jsonBytes);
            var content = new ByteArrayContent(encrypted);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var response = await _httpClient.PostAsync("profiles", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<List<Profile>> LoadProfilesAsync()
        {
            var response = await _httpClient.GetAsync("profiles");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<Profile>();

            response.EnsureSuccessStatusCode();
            byte[] encrypted = await response.Content.ReadAsByteArrayAsync();
            byte[] decrypted = await DecryptAsync(encrypted);
            string json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
        }

        private async Task SaveTodosAsync(Guid userId, IEnumerable<TodoItem> todos)
        {
            string json = JsonSerializer.Serialize(todos, new JsonSerializerOptions { WriteIndented = false });
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            byte[] encrypted = await EncryptAsync(jsonBytes);
            var content = new ByteArrayContent(encrypted);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            var response = await _httpClient.PostAsync($"todos/{userId}", content);
            response.EnsureSuccessStatusCode();
        }

        private async Task<List<TodoItem>> LoadTodosAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync($"todos/{userId}");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new List<TodoItem>();

            response.EnsureSuccessStatusCode();
            byte[] encrypted = await response.Content.ReadAsByteArrayAsync();
            byte[] decrypted = await DecryptAsync(encrypted);
            string json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<List<TodoItem>>(json) ?? new List<TodoItem>();
        }

        private static async Task<byte[]> EncryptAsync(byte[] plainData)
        {
            using var ms = new MemoryStream();
            using var aes = CryptoConfig.CreateAes();
            using var crypto = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await crypto.WriteAsync(plainData, 0, plainData.Length);
            await crypto.FlushFinalBlockAsync();
            return ms.ToArray();
        }

        private static async Task<byte[]> DecryptAsync(byte[] encryptedData)
        {
            using var msInput = new MemoryStream(encryptedData);
            using var msOutput = new MemoryStream();
            using var aes = CryptoConfig.CreateAes();
            using var crypto = new CryptoStream(msInput, aes.CreateDecryptor(), CryptoStreamMode.Read);
            await crypto.CopyToAsync(msOutput);
            return msOutput.ToArray();
        }
    }
}