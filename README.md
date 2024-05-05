# Onx Graph

An application to visualize relationships between [Onspring](https://onspring.com/) apps.

## Server

The server is a .NET web api that exposes a RESTful API for the front-end to consume and uses SignalR to support real time communication with the client. It connects to a MongoDB database to store and retrieve data.

## Client

The client is a Vue.js application that consumes the server's API. It uses [Pinia](https://pinia.vuejs.org/) for state management and [Vue Router](https://router.vuejs.org/) for routing.

## Technologies

The application is built using:

- [Vue.js](https://vuejs.org/) for the front-end
- [Vite](https://vitejs.dev/) for the front-end build tool
- [Vitest](https://vitest.dev/) for unit and integration testing in the front-end
- [Pinia](https://pinia.vuejs.org/) for state management in the front-end
- [Vue Router](https://router.vuejs.org/) for routing in the front-end
- [Playwright](https://playwright.dev/) for end-to-end testing
- [.NET](https://dotnet.microsoft.com/) for the back-end
- [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr) for real-time communication
- [MongoDB](https://www.mongodb.com/) for the database
- [Serilog](https://serilog.net/) for logging in the back-end
- [xUnit](https://xunit.net/) for unit and integration testing in the back-end
- [Moq](https://github.com/devlooped/moq) for mocking in the back-end
- [Bogus](https://github.com/bchavez/Bogus) for generating fake data in the back-end
- [Testcontainers](https://www.testcontainers.org/) for integration testing in the back-end
- [Swagger](https://swagger.io/) for API documentation and testing in development
- [Docker](https://www.docker.com/) for containerization

## Authentication

The application uses [Json Web Tokens](https://jwt.io/) for authentication. A user logs in using their email and password and upon a successful attempt the server generates an access token and returns it to the caller, which is then passed to the front-end. The front-end stores the token in local storage and passes it in the `Authorization` header when making a request to the server that requires authentication. The server validates the token and returns a 401 if it is invalid. The server also exposes an endpoint to refresh the token, which can be called by the front-end when the access token expires to generate a new access token. The refresh token is stored in an http-only, secure cookie and is issued when the user logs in and is rotated each time the user refreshes their access token.
