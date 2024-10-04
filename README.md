# Simple HTTP Server

This project implements a simple HTTP server that handles various requests using the curl command. It supports basic HTTP methods like GET and POST, along with custom features such as file handling, gzip compression, and concurrent connections. The server responds to different endpoints with various functionalities:

- **Basic HTTP response**: Returns a `200 OK` for valid requests and a `404 Not Found` for invalid paths.
- **Echo endpoint**: Responds with the requested path in the response body.
- **Custom headers**: Handles User-Agent headers and returns them in the response body.
- **File serving**: Allows serving files from a specified directory and creates new files through POST requests.
- **Gzip compression**: Supports compression when requested via the `Accept-Encoding` header.
- **Concurrent connections**: Handles multiple client requests simultaneously, responding with appropriate status codes and headers.

This project demonstrates the basic workings of an HTTP server with features like request handling, file I/O, and content compression, making it a good learning tool for understanding HTTP communication and server functionality.
