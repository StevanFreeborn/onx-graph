global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Net.Http.Json;
global using System.Net.Mail;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.RegularExpressions;

global using Bogus;

global using DotNet.Testcontainers.Builders;
global using DotNet.Testcontainers.Containers;

global using FluentAssertions;

global using FluentResults;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using MongoDB.Bson;
global using MongoDB.Driver;

global using Moq;

global using Server.API.Authentication;
global using Server.API.Configuration;
global using Server.API.Email;
global using Server.API.Identity;
global using Server.API.Persistence.Mongo;
global using Server.API.Tests.Data;
global using Server.API.Tests.Integration.Fixtures;
global using Server.API.Tests.Integration.Utilities;

global using Testcontainers.MongoDb;

global using Xunit;
global using Microsoft.Extensions.Time.Testing;