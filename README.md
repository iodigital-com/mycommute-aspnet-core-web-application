# MyCommute - Implementing Web API

To expose the business logic implemented in the previous module, you will create a new [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api) project. 

First you will be guided through the structure & components that make up a typical ASP.NET Core application. Concepts like Dependency Injection (DI), host & application bootstrapping, configuration scope will be discussed.
Then you will implement a RESTful API for CRUD functionality.

[more](https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/)

## Topics
- [WebApplicationBuilder](https://andrewlock.net/exploring-dotnet-6-part-2-comparing-webapplicationbuilder-to-the-generic-host/#asp-net-core-6-webapplicationbuilder-)
- [Dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Environment variables](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Project references](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-reference)
- [REST](https://restfulapi.net/)
- [Swashbuckle](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle) Swagger API Documentation

## Prerequisites
You'll need to set up your machine to run .NET, including the C# 10.0 compiler. [Download](https://dotnet.microsoft.com/download/dotnet/6.0) & install the .NET 6 SDK.

You'll need a connection string to an empty [SQL Server database](https://www.microsoft.com/en-us/sql-server/developer-get-started/).

## Requirements

We are going to create an application that keeps track of the modes of transport that a company's employees use to commute to work daily. The full solution will include:
- Data project: defines the data structure
- Domain project: business logic implementations
- WebApplication project: includes the web API's & backoffice web application to manage the employee data
- Mobile app project: will be used by the employees to input the mode of transport for their daily commute

## Assignments

### Create a new ASP.NET Core Web Application project
Fork the starter solution from version control, clone it to your local environment and create a new [ASP.NET Core Web Application](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api) project and name it `MyCommute.WebApplication`. 

```
dotnet new webapi --auth None --language "C#" --name MyCommute.WebApplication
```

### Remove the demo content
The [ASP.NET Core Web Application](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api) project template ships with some demo content, which we won't need. 

- Delete the `WeatherForecast` class (located in the root of the web application project).
- Then, using the IDE's refactor functionality, rename the `WeatherForecastController` to `UserController`.
- Remove the `Summaries` field & the `Get` method from `UserController`.

### Reference other projects
Add [project references](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-add-reference) to the `MyCommute.WebApplication` project.

There are several ways you can do this. Your IDE probably has a feature that allows you to add project references from the GUI.
Or you can add references to a project using CLI:

Open a terminal, `cd` to the root of the solution and run the following commands:

```
dotnet add MyCommute.WebApplication/MyCommute.WebApplication.csproj reference MyCommute.Data/MyCommute.Data.csproj
```

```
dotnet add MyCommute.WebApplication/MyCommute.WebApplication.csproj reference MyCommute.Domain/MyCommute.Domain.csproj
```

Open the `MyCommute.WebApplication.csproj` file. Verify that it includes references to the `MyCommuet.Data` & `MyCommute.Domain` projects:

```xml
<ItemGroup>
  <ProjectReference Include="..\MyCommute.Data\MyCommute.Data.csproj" />
  <ProjectReference Include="..\MyCommute.Domain\MyCommute.Domain.csproj" />
</ItemGroup>
```

### Install NuGet packages
Install the latest version of the following NuGet packages:
- [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore/)
- [Microsoft.EntityFrameworkCore.SqlServer](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.SqlServer)

### Configure dependency injection in Program.cs
> ASP.NET Core supports the dependency injection (DI) software design pattern, which is a technique for achieving Inversion of Control (IoC) between classes and their dependencies.
> https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection

In order to use the services from the `MyCommute.Domain` project, you have to register them in `Program.cs`

After `var builder = WebApplication.CreateBuilder(args);` add 
```c#
builder.Services.AddTransient<IEmployeeService, EmployeeService>();
```
This registers the `EmployeeService` with a [Transient](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#transient) lifetime.
> Transient lifetime services are created each time they're requested from the service container. This lifetime works best for lightweight, stateless services. Register transient services with AddTransient.
> In apps that process requests, transient services are disposed at the end of the request.

More information on [service lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes).

Also register `CommuteService` & `GeoCodeService` with a [Transient](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#transient) lifetime.

### Configure database connection in Program.cs
The web application needs access to a database. To register the `MyCommute.Data.DataContext` instance as a service add this to `Program.cs`

```c#
builder.Services.AddDbContextFactory<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer"),
        x =>
        {
            x.UseNetTopologySuite();
        })
);
```
Let's break this down:
- `AddDbContextFactory<DataContext>` : Registers an `IDbContextFactory<TContext>` in the `IServiceCollection` to create instances of given `DbContext` type.
- `options.UseSqlServer(builder.Configuration.GetConnectionString("sqlServer")` : indicates we want to use the [Sql Server database provider](https://docs.microsoft.com/en-us/ef/core/providers/sql-server/). The name of the connection string is passed in to the context by calling a method on a `DbContextOptionsBuilder` object. For local development, the ASP.NET Core configuration system reads the connection string from the `appsettings.Development.json` file.
- `x.UseNetTopologySuite();` : specifies we want to use NetTopologySuite to access SQL Server [spatial data](https://aka.ms/efcore-docs-spatial).

Open the `appsettings.Development.json` file and add a connection string as shown in the following markup:

```json
"ConnectionStrings": {
    "sqlServer": "{{insert connection string here}}"
  }
```

### Inspect the OpenAPI Specification
The file `openapi-specification.json` (located in the root of the solution) describes the API you are going to implement. Open the file in [Swagger Editor](https://editor.swagger.io/)

### Implement UserController
The `UserController` will provide functionality concerning CRUD operations for the users of the mobile app. 

Features include:
- User registration
- Updating user information
- Deleting a user account

Conforming to the constraints of the [Representational State Transfer (REST)](https://restfulapi.net/rest-api-design-tutorial-with-example/) architectural style, implement an endpoint for each of the features mentioned above.

#### Declare the request & response models
In the `MyCommute.Shared` class library project, navigate to directory `Models` and create a new directory `User`. 

Using C# immutable [records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record), declare the following models in namespace `MyCommute.Shared.Models.User`:

- `UserRegistrationRequest`, with properties:
  
  - `Name` (`string`)
  - `Email` (`string`)
  - `HomeAddress` (`Address`, already defined in `MyCommute.Shared.Models`)
  - `WorkAddress` (`Address`)
  - `DefaultCommuteType` (`CommuteType`, defined in namespace `MyCommute.Shared.Enums`)
- `UserRegistrationResponse`, with properties:
  
  - `Id` (`Guid`)
- `UserUpdateRequest`, with properties:
  
  - `Id` (`Guid`)
  - `Name` (`string`)
  - `HomeAddress` (`Address`, already defined in `MyCommute.Shared.Models`)
  - `WorkAddress` (`Address`)
  - `DefaultCommuteType` (`CommuteType`, defined in namespace `MyCommute.Shared.Enums`)

#### Implement the user registration endpoint
Registering an account is an operation that creates a new resource. Implement a new method `Register`, annotated with [`HttpPostAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httppostattribute). This attribute signals that the endpoint only accepts POST requests.

```c#
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<UserRegistrationResponse>> Register(UserRegistrationRequest request)
```

The `Register` method accepts 1 parameter of type `UserRegistrationRequest`, and returns `Task<ActionResult<UserRegistrationResponse>>`.
This method creates a new `Employee` entity, using the `UserRegistrationRequest` properties. The `HomeAddress` & `WorkAddress` properties are geocoded to their respective geographical coordinates,  using the `GeoCodeService.GetCoordinatesForAddressAsync` method.

The `Employee` entity is then added to the database using the `EmployeeService.AddAsync` method. If this operation succeeds, a `UserRegistrationResponse` is returned, containing the value of the `Employee.Id` property of the newly added `Employee`.

If any exception is encountered in the process, the exception is logged and a [`BadRequestResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.badrequest) is returned.

#### Test the user registration endpoint with Swashbuckle
The [ASP.NET Core Web Application](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api) project template ships with [Swashbuckle](https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle) installed & configured. The Swagger UI tool is an easy way to debug the api endpoint you just created. Simply run the WebApplication project and a browser window will open. Here you can test sending a request to the endpoint with the **Try it out** button.

#### Implement the update user endpoint
Updating an account is an operation that mutates a resource. Implement a new method `Update`, annotated with [`HttpPutAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpputattribute). This attribute signals that the endpoint only accepts PUT requests.

```c#
[HttpPut]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> Update(UserUpdateRequest request)
```

The `Update` method accepts 1 parameter of type `UserUpdateRequest`, and returns `Task<ActionResult>`.
This method creates a new `Employee` entity, using the `UserUpdateRequest` properties. The `HomeAddress` & `WorkAddress` properties are geocoded to their respective geographical coordinates, using the `GeoCodeService.GetCoordinatesForAddressAsync` method.

The `Employee` entity is then updated in the database using the `EmployeeService.UpdateAsync` method. If this operation succeeds, an [`OkResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.ok) is returned.

If no employee matching the `UserUpdateRequest.EmployeeId` property is found in the database, a [`NotFoundResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.notfound) is returned.

If any other exceptions are encountered, the exception is logged and a [`BadRequestResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.badrequest) is returned.

#### Implement the delete user endpoint
Implement a new method `Delete` annotated with [`HttpDeleteAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpdeleteattribute). This attribute signals that the endpoint only accepts DELETE requests.

```c#
[HttpDelete]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> Delete(Guid id)
```

The `Delete` method accepts 1 parameter of type `Guid`, and returns `Task<ActionResult>`.
The `Employee` entity matching the provided Id is removed from the database using the `EmployeeService.DeleteAsync` method. If this operation succeeds, an [`OkResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.ok) is returned.

If no employee matching the provided id is found in the database, a [`NotFoundResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.notfound) is returned.

### Implement CommuteController
The `CommuteController` will provide functionality concerning CRUD operations for the daily commutes of users.

Features include:
- Get a list of all commutes for a particular user
- Add a commute
- Update a commute
- Delete a commute

#### Declare the request & response models
In the `MyCommute.Shared` class library project, navigate to directory `Models` and create a new directory `Commute`.

Using C# immutable [records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record), declare the following models in namespace `MyCommute.Shared.Models.Commute`:

- `AddCommuteRequest`, with properties:
  
  - `EmployeeId` (`Guid`)
  - `ModeOfTransport` (`Enums.CommuteType`)
  - `Date` (`DateTime`)
- `UpdateCommuteRequest`, with properties:

  - `Id` (`Guid`)
  - `ModeOfTransport` (`Enums.CommuteType`)
  - `Date` (`DateTime`)
- `AddOrUpdateCommuteResponse`, with properties:
  
  - `Id` (`Guid`)
  - `ModeOfTransport` (`Enums.CommuteType`)
  - `Date` (`DateTime`)

#### Implement the get commutes endpoint

Implement a new method `Get` annotated with [`HttpGetAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpgetattribute)). This attribute signals that the endpoint only accepts GET requests.

```c#
[HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<IEnumerable<CommuteDto>>> Get(Guid employeeId)
```
The `Get` method accepts 1 parameter of type `Guid`, and returns `Task<ActionResult<IEnumerable<CommuteDto>>>`.

The `CommuteDto` type is a [Data Transfer Object (DTO)](https://stackoverflow.com/a/1058186), that is used to represent some properties of the `Commute` entity.
In namespace `MyCommute.Shared.Models.Commute` add a new record `CommuteDto`, with properties:
- `Id` (`Guid`)
- `EmployeeId` (`Guid`)
- `ModeOfTransport` (`Enums.CommuteType`)
- `Date` (`DateTime`)

This method calls `CommuteService.GetByUserIdAsync` and maps the returned `Commute` collection to a collection of `CommuteDto` objects.
If no commutes are found for the provided (employee) id, a [`NotFoundResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.notfound) is returned.

If any other exceptions are encountered, the exception is logged and a [`BadRequestResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.badrequest) is returned.

#### Implement the add commute endpoint
Implement a new method `Add`, annotated with [`HttpPostAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httppostattribute). This attribute signals that the endpoint only accepts POST requests.

```c#
[HttpPost]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<AddOrUpdateCommuteResponse>> Add(AddCommuteRequest request)
```
The `Add` method accepts 1 parameter of type `AddCommuteRequest`, and returns `Task<ActionResult<AddOrUpdateCommuteResponse>>`.
This method maps the properties of the `AddCommuteRequest` object to a new `Commute` entity, which is then persisted to the datastore using the `CommuteService.AddAsync` method.

If this operation succeeds, an `AddOrUpdateCommuteResponse` is returned.
If any exception is encountered in the process, the exception is logged and a [`BadRequestResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.badrequest) is returned.

#### Implement the update commute endpoint
Implement a new method `Update`, annotated with [`HttpPutAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.httpputattribute). This attribute signals that the endpoint only accepts PUT requests.

```c#
[HttpPut]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<AddOrUpdateCommuteResponse>> Update(UpdateCommuteRequest request)
```

The `Update` method accepts 1 parameter of type `UpdateCommuteRequest`, and returns `Task<ActionResult<AddOrUpdateCommuteResponse>>`.
This method maps the properties of the `UpdateCommuteRequest` object to a new `Commute` entity, which is then persisted to the datastore using the `CommuteService.UpdateAsync` method.

If this operation succeeds, an `AddOrUpdateCommuteResponse` is returned.
If any exception is encountered in the process, the exception is logged and a [`BadRequestResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.badrequest) is returned.

#### Implement the delete commute endpoint

```c#
[HttpDelete]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> Delete(Guid id)
```

The `Delete` method accepts 1 parameter of type `Guid`, and returns `Task<ActionResult>`.
The `Commute` entity matching the provided Id is removed from the database using the `CommuteService.DeleteAsync` method. If this operation succeeds, an [`OkResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.ok) is returned.

If no commute matching the provided id is found in the database, a [`NotFoundResult`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.notfound) is returned.

