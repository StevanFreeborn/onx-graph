# Onx Graph

An application to visualize relationships between [Onspring](https://onspring.com/) apps.

## Technologies

The application is built using:

- [Vue.js](https://vuejs.org/) for the front-end
- [Vite](https://vitejs.dev/) for the front-end build tool
- [.NET](https://dotnet.microsoft.com/) for the back-end
- [MongoDB](https://www.mongodb.com/) for the database
- [Serilog](https://serilog.net/) for logging
- [xUnit](https://xunit.net/) for unit and integration testing
- [Moq](https://github.com/devlooped/moq) for mocking
- [Bogus](https://github.com/bchavez/Bogus) for generating fake data
- [Testcontainers](https://www.testcontainers.org/) for integration testing
- [Swagger](https://swagger.io/) for API documentation

## Authentication

The application uses [Json Web Tokens](https://jwt.io/) for authentication. A user logs in using their email and password and upon a successful attempt the server generates an access token and returns it to the caller, which is then passed to the front-end. The front-end stores the token in local storage and passes it in the `Authorization` header of when making a request to the server that requires authentication. The server validates the token and returns a 401 if it is invalid. The server also exposes an endpoint to refresh the token, which can be called by the front-end when the access token expires to generate a new access token. The refresh token is stored in an http-only, secure cookie and is issued when the user logs in and is rotated each time the user refreshes their access token.

## Server

The server is a .NET web api that exposes a RESTful API for the front-end to consume. It connects to a MongoDB database to store and retrieve data.
