global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Reflection;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;

global using Asp.Versioning;

global using FluentResults;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Any;
global using Microsoft.OpenApi.Models;

global using MongoDB.Bson.Serialization;
global using MongoDB.Bson.Serialization.IdGenerators;
global using MongoDB.Driver;

global using Serilog;
global using Serilog.Formatting.Compact;
global using Serilog.Sinks.SystemConsole.Themes;

global using Server.API.Authentication;
global using Server.API.Configuration;
global using Server.API.Identity;
global using Server.API.Middleware;
global using Server.API.Persistence.Mongo;
global using Server.API.Swagger;

global using Swashbuckle.AspNetCore.SwaggerGen;