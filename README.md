# Secora

Secora is a dynamic, decoupled middleware and dashboard platform. It features a robust plugin architecture that allows new capabilities and UI components to be discovered and loaded at runtime without recompiling the host application.

## Documentation

<details>
<summary><b>Plugin Development Guidelines</b> (Click to Expand)</summary>

Secora features a dynamic, decoupled plugin architecture. This means you can create plugins as standalone Razor Class Libraries that are loaded into the main application dynamically at runtime.

Follow these guidelines to create a reliable and visually consistent plugin.

---

### 1. Project Setup

Create a new .NET 9 project for your plugin. The project should end with `.SecoraPlugin` to be picked up by the loader (e.g., `MyAwesome.SecoraPlugin`).

Modify your `.csproj` file to target the `Microsoft.NET.Sdk.Razor` SDK and add a reference to `Secora.Abstractions`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.18" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Secora.Abstractions\Secora.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

---

### 2. Implement the Plugin Interface

Create a class that implements `ISecoraPlugin`. This serves as the entry point for your plugin.

```csharp
using Secora.Abstractions;

namespace MyAwesome.SecoraPlugin;

public class MyAwesomePlugin : ISecoraPlugin
{
    public string Name { get; set; } = "My Awesome";
    public Version Version { get; set; } = new Version(1, 0, 0);
    public string Author => "Your Name";
    public string Description => "A description of what your plugin does.";
    public IPluginUiPage Page => new MyAwesomeUiPage();
}
```

---

### 3. Create the UI Component

Create a Blazor component (e.g., `MyAwesomePage.razor`) that will render your plugin's user interface. 

> [!IMPORTANT]
> **Root Container Class**: You must wrap your entire component in a root `<div>` where the class name is your plugin's `Name` property in **lowercase with all spaces removed**. (e.g., "My Awesome" -> `myawesome`).

```razor
@using Microsoft.AspNetCore.Components

<div class="myawesome">
    <h2>My Awesome Plugin</h2>
    <p>Hello from the dynamic plugin!</p>
    <button class="sp-btn sp-btn-primary">Click Me</button>
</div>
```

---

### 4. Register the UI Page

Create a class implementing `IPluginUiPage` that points to your Blazor component. This is returned by your `ISecoraPlugin` implementation.

```csharp
using Secora.Abstractions;

namespace MyAwesome.SecoraPlugin;

public class MyAwesomeUiPage : IPluginUiPage
{
    public string Title => "My Awesome Plugin";
    public Type ComponentType => typeof(MyAwesomePage);
}
```

---

### 5. CSS Styling & Isolation

Secora uses a powerful **CSS Nesting** architecture to automatically isolate your plugin's styles from the rest of the application. 

You do **not** need to use Blazor's standard CSS isolation (`.razor.css`). Instead:

1. Create a raw CSS file in your plugin project (e.g., `MyAwesomePlugin.css`).
2. Write your standard CSS *without* worrying about global scope bleeding:
   ```css
   display: flex;
   flex-direction: column;

   h2 {
       color: #16a34a;
   }
   ```
3. Add the CSS file to your `.csproj` as an `<EmbeddedResource>`:
   ```xml
   <ItemGroup>
       <EmbeddedResource Include="MyAwesomePlugin.css" />
   </ItemGroup>
   ```

**How it works:** When the Secora `PluginManager` loads your plugin, it dynamically extracts this embedded CSS and wraps it in a CSS nested block matching your plugin's name (`.myawesome { ... }`). Because you wrapped your Razor component in `<div class="myawesome">`, all your styles will perfectly apply *only* to your plugin!

---

### 6. Testing & Deployment

To deploy your plugin locally:
1. Run `dotnet build` on your plugin project.
2. Copy the resulting `MyAwesome.SecoraPlugin.dll` file into the host application's output directory (e.g., `empService/bin/Debug/net9.0/`).
3. Start the host application (`dotnet run`).

The `PluginManager` will automatically discover your DLL, aggregate your CSS, and mount your Blazor component into the Secora UI!

</details>
