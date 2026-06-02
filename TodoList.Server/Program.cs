using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TodoList.Server
{
    class Program
    {
        private static readonly string DataDirectory = "server_data";
        private static readonly object FileLock = new object();

        static async Task Main(string[] args)
        {
            Directory.CreateDirectory(DataDirectory);

            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/");
            listener.Start();
            Console.WriteLine("Сервер запущен на http://localhost:5000/");
            Console.WriteLine("Ожидание запросов...");

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequestAsync(context));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
            }
        }

        private static async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                string path = request.Url?.AbsolutePath ?? "";
                string method = request.HttpMethod;

                Console.WriteLine($"{method} {path}");

                if (method == "POST" && path == "/profiles")
                {
                    await SaveDataToFile(request, response, "server_profiles.dat");
                }
                else if (method == "GET" && path == "/profiles")
                {
                    await SendFileContent(response, "server_profiles.dat");
                }
                else if (method == "POST" && path.StartsWith("/todos/"))
                {
                    string userId = path.Substring("/todos/".Length);
                    string fileName = $"server_todos_{userId}.dat";
                    await SaveDataToFile(request, response, fileName);
                }
                else if (method == "GET" && path.StartsWith("/todos/"))
                {
                    string userId = path.Substring("/todos/".Length);
                    string fileName = $"server_todos_{userId}.dat";
                    await SendFileContent(response, fileName);
                }
                else
                {
                    response.StatusCode = 404;
                    byte[] error = Encoding.UTF8.GetBytes("Not Found");
                    response.ContentLength64 = error.Length;
                    await response.OutputStream.WriteAsync(error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки: {ex.Message}");
                response.StatusCode = 500;
                byte[] error = Encoding.UTF8.GetBytes("Internal Server Error");
                response.ContentLength64 = error.Length;
                await response.OutputStream.WriteAsync(error);
            }
            finally
            {
                response.Close();
            }
        }

        private static async Task SaveDataToFile(HttpListenerRequest request, HttpListenerResponse response, string fileName)
        {
            string filePath = Path.Combine(DataDirectory, fileName);

            using var ms = new MemoryStream();
            await request.InputStream.CopyToAsync(ms);
            byte[] data = ms.ToArray();

            lock (FileLock)
            {
                File.WriteAllBytes(filePath, data);
            }

            response.StatusCode = 200;
            byte[] ok = Encoding.UTF8.GetBytes("OK");
            response.ContentLength64 = ok.Length;
            await response.OutputStream.WriteAsync(ok);
        }

        private static async Task SendFileContent(HttpListenerResponse response, string fileName)
        {
            string filePath = Path.Combine(DataDirectory, fileName);

            if (!File.Exists(filePath))
            {
                response.StatusCode = 404;
                byte[] notFound = Encoding.UTF8.GetBytes("Not Found");
                response.ContentLength64 = notFound.Length;
                await response.OutputStream.WriteAsync(notFound);
                return;
            }

            byte[] data;
            lock (FileLock)
            {
                data = File.ReadAllBytes(filePath);
            }

            response.StatusCode = 200;
            response.ContentType = "application/octet-stream";
            response.ContentLength64 = data.Length;
            await response.OutputStream.WriteAsync(data);
        }
    }
}