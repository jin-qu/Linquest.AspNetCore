# Linquest.AspNetCore
Linquest Asp.Net Core backend.

[![Build and Test](https://github.com/umutozel/Linquest.AspNetCore/actions/workflows/build.yml/badge.svg)](https://github.com/umutozel/Linquest.AspNetCore/actions/workflows/build.yml)
[![codecov](https://codecov.io/gh/umutozel/Linquest.AspNetCore/graph/badge.svg?token=5A9hHTDVFc)](https://codecov.io/gh/umutozel/Linquest.AspNetCore)
[![NuGet Badge](https://img.shields.io/nuget/v/Linquest.AspNetCore.svg)](https://www.nuget.org/packages/Linquest.AspNetCore/)
![NuGet Downloads](https://img.shields.io/nuget/dt/Linquest.AspNetCore.svg)
[![GitHub issues](https://img.shields.io/github/issues/jin-qu/Linquest.AspNetCore.svg)](https://github.com/jin-qu/Linquest.AspNetCore/issues)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/jin-qu/Linquest.AspNetCore/master/LICENSE)

[![GitHub stars](https://img.shields.io/github/stars/jin-qu/Linquest.AspNetCore.svg?style=social&label=Star)](https://github.com/jin-qu/Linquest.AspNetCore)
[![GitHub forks](https://img.shields.io/github/forks/jin-qu/Linquest.AspNetCore.svg?style=social&label=Fork)](https://github.com/jin-qu/Linquest.AspNetCore)

# Installation

#### Package Manager
```
Install-Package Linquest.AspNetCore
```
#### .Net CLI
```
dotnet add package Linquest.AspNetCore
```

# Getting Started

To add support for linquest queries, you need LinquestActionFilter.

To use callbacks and customizing the request processing;
#### 1. You can inherit your Controller from LinquestController
```csharp
public class TestController : LinquestController {

  public IQueryable<Company> Companies() {
    // ...
  }
}
```
#### 2. You can implement ILinquestService interface
```csharp
public class TestController : ILinquestService {

  public IQueryable<Company> Companies() {
    // ...
  }
  
  // limit the maximum allowed result count, exception will be thrown if given value is exceeded
  public int? MaxResultCount { get; set; }
  
  public event BeforeQueryDelegate BeforeHandleQuery;
  public event BeforeQueryDelegate BeforeQueryExecute;
  public event AfterQueryDelegate AfterQueryExecute;

  void ILinquestService.OnBeforeHandleQuery(BeforeQueryEventArgs args) 
      => BeforeHandleQuery?.Invoke(this, args);

  void ILinquestService.OnBeforeQueryExecute(BeforeQueryEventArgs args) 
      => BeforeQueryExecute?.Invoke(this, args);

  void ILinquestService.OnAfterQueryExecute(AfterQueryEventArgs args) 
      => AfterQueryExecute?.Invoke(this, args);

  public ProcessResult ProcessRequest(ActionContext context) {
    // you can customize processing the request, or use default processor
    return Helper.DefaultRequestProcessor(context, this.HttpContext.RequestServices);
  }
}
```

If you don't need intercepting the Linquest operations, you can use LinquestActionFilter;
#### 1. With Controller
```csharp
[LinquestActionFilter]
public class Test2Controller {

  public IQueryable<Company> Companies() {
    // ...
  }
}
```
#### 2. With Action
```csharp
public class Test2Controller {

  [LinquestActionFilter]
  public IQueryable<Company> Companies() {
    // ...
  }
}
```

# Processing Values
Linquest handles IQueryable values by default, using QueryableHandler class.
You can change handlers by injecting alternative implementations.

#### Add new IQueryable handler at Startup
```csharp
public override void ConfigureServices(IServiceCollection services) {
  // ...
  services.AddSingleton<IContentHandler<IQueryable>, ProductHandler>();
}
```
#### Add a handler for custom type, like PetaPoco IQuery
```csharp
public override void ConfigureServices(IServiceCollection services) {
  // ...
  services.AddSingleton<IContentHandler<PetaPoco.IQuery>, PetaPocoQueryHandler>();
}

// ...

public class PetaPocoQueryHandler: IContentHandler<PetaPoco.IQuery> {

  public virtual ProcessResult HandleContent(object query, ActionContext context) {
      return HandleContent((PetaPoco.IQuery)query, context);
  }

  public virtual ProcessResult HandleContent(PetaPoco.IQuery query, ActionContext context) {
    // ...
  }
}
```
This way, you can apply linquest query parameters to any type.

# License
Linquest.AspNetCore is under the [MIT License](LICENSE).

