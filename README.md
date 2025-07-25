# TodoItems API Tests

This project contains comprehensive API tests for the TodoItems REST API, covering all CRUD operations (Create, Read, Update, Delete) with various scenarios including positive and negative test cases.

## Project Structure

The test suite is organized into different test classes based on HTTP methods:

- **TodoItemsGetTests**: Tests for GET operations
- **TodoItemsPostTests**: Tests for POST operations  
- **TodoItemsPutTests**: Tests for PUT operations
- **TodoItemsDeleteTests**: Tests for DELETE operations

## Prerequisites

### Required Software

1. **.NET 7.0 SDK or higher**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: `dotnet --version`

2. **VS Code** (optional but recommended)
   - For VS Code: Install C# extension

### Required NuGet Packages

The following packages are automatically restored when building the project, but you can also install them manually using the commands below:

#### Core Testing Framework
```bash
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package MSTest.TestAdapter
dotnet add package MSTest.TestFramework
```

#### HTTP Client Library
```bash
dotnet add package RestSharp
```

#### Assertion Library
```bash
dotnet add package FluentAssertions
```

#### Alternative: Install All Packages at Once
```bash
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package MSTest.TestAdapter
dotnet add package MSTest.TestFramework
dotnet add package RestSharp
dotnet add package FluentAssertions
```

#### Verify Installed Packages
```bash
dotnet list package
```

#### Update Packages to Latest Versions
```bash
dotnet add package Microsoft.NET.Test.Sdk --version latest
dotnet add package MSTest.TestAdapter --version latest
dotnet add package MSTest.TestFramework --version latest
dotnet add package RestSharp --version latest
dotnet add package FluentAssertions --version latest
```


## Environment Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ApiTests
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Project

```bash
dotnet build
```

### 4. Start the API Server

See details in [GitHub - seymenbahtiyar/Web_API_Demo: This is a sample web API project using ASP.NET Core and Entity Framework. It allows you to create, read, update and delete (CRUD) todo items from a PostgreSQL database. For testing, the InMemory database can be used.](https://github.com/seymenbahtiyar/Web_API_Demo)

### 5. Running Tests

#### 5.1 Run All Tests

```bash
dotnet build
dotnet test
```



#### 5.2 Generate Test Reports

#### Generate HTML Report (using trxlog2html)

```bash
dotnet tool install --global trxlog2html --version 1.0.4 
dotnet test --logger "trx;LogFileName=test_results.trx"
trxlog2html -i TestResults/test_results.trx -o ./TestReport/test_results.html
```



**Report**:
https://github.com/nightwish2016/Homework_API_TEST/blob/main/ApiTests/TestReport/test_results.html
https://github.com/nightwish2016/Homework_API_TEST/blob/main/report.png






 
