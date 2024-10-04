using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Compression;

TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
Console.WriteLine("Server started. Listening on port 4221...");

while (true)
{
    // Accept a new client connection
    var clientSocket = server.AcceptSocket();

    // Handle each client connection in a separate thread
    Thread clientThread = new Thread(() => HandleClient(clientSocket));
    clientThread.Start();
}

static void HandleClient(Socket clientSocket)
{
    try
    {
        var responseBuffer = new byte[1024];
        int receivedBytes = clientSocket.Receive(responseBuffer);
        var lines = Encoding.UTF8.GetString(responseBuffer).Split("\r\n");

        var line0Parts = lines[0].Split(" ");
        var (method, path, httpVer) = (line0Parts[0], line0Parts[1], line0Parts[2]);

        // Extract the Accept-Encoding value
        bool gzipEncoded = false;
        for (int i = 1; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("Accept-Encoding: "))
            {
                var encodings = lines[i].Substring(17);
                // Check for "gzip" presence (case-insensitive)
                if (encodings.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    gzipEncoded = true;
                }
                break;
            }
        }

        Console.WriteLine($"Received request for path: {path}");
        byte[] responseBody = Array.Empty<byte>();
        string headers = $"{httpVer} 200 OK\r\n";

        if (path == "/")
        {
            headers += "\r\n"; // Empty response for the root path
        }
        else if (path.StartsWith("/echo/"))
        {
            string message = path.Substring(6);
            responseBody = Encoding.UTF8.GetBytes(message); // Prepare the response body

            if (gzipEncoded)
            {
                using (var compressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(responseBody, 0, responseBody.Length);
                    }
                    responseBody = compressedStream.ToArray();
                    headers += "Content-Encoding: gzip\r\n";
                }
            }

            headers += $"Content-Type: text/plain\r\nContent-Length: {responseBody.Length}\r\n\r\n";
        }
        else if (path == "/user-agent")
        {
            string userAgent = string.Empty;

            // Loop through the headers and find the User-Agent header
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("User-Agent: "))
                {
                    userAgent = lines[i].Substring(12); // Extract value after "User-Agent: "
                    break;
                }
            }

            if (!string.IsNullOrEmpty(userAgent))
            {
                responseBody = Encoding.UTF8.GetBytes(userAgent);
                headers += $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {responseBody.Length}\r\n\r\n";
            }
            else
            {
                headers = $"{httpVer} 400 Bad Request\r\n\r\n"; // Handle missing User-Agent
            }
        }
        else if (path.Contains("/files/"))
        {
            // Get the directory path from command-line arguments
            var directory = Environment.GetCommandLineArgs()[2];
            Console.WriteLine($"Directory: {directory}");

            // Extract the filename from the path
            var fileName = path.Split("/")[2];
            var pathFile = Path.Combine(directory, fileName);
            if (method == "GET")
            {
                // Check if the file exists in the directory
                if (File.Exists(pathFile))
                {
                    // Read the file content
                    responseBody = File.ReadAllBytes(pathFile);
                    headers += $"{httpVer} 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {responseBody.Length}\r\n\r\n";
                }
                else
                {
                    // File not found response
                    headers = $"{httpVer} 404 Not Found\r\n\r\n";
                }
            }
            else if (method == "POST")
            {
                string fileContent = lines[lines.Length - 1];
                File.WriteAllText(pathFile, fileContent);
                headers += "HTTP/1.1 201 Created\r\n\r\n";
                Console.WriteLine($"File {fileName} created with content : {fileContent}");
            }
        }
        else
        {
            headers = $"{httpVer} 404 Not Found\r\n\r\n";
        }

        // Combine headers and body into one byte array and send
        var fullResponse = Encoding.UTF8.GetBytes(headers).Concat(responseBody).ToArray();
        clientSocket.Send(fullResponse);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error handling client: {ex.Message}");
    }
}
