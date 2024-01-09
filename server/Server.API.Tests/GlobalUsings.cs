global using System.Net;

global using Bogus;

global using FluentAssertions;

global using FluentResults;

global using FluentValidation;
global using FluentValidation.Results;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using Moq;

global using Server.API.Authentication;
global using Server.API.Identity;
global using Server.API.Persistence.Mongo;
global using Server.API.Tests.Integration.Fixtures;

global using Testcontainers.MongoDb;

global using Xunit;
global using Microsoft.AspNetCore.Http;
