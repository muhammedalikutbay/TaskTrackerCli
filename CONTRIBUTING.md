# Contributing

Thanks for helping improve Task Tracker CLI!

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download).
2. `dotnet build` and `dotnet test` must pass (warnings are errors).
3. Keep the app free of third-party libraries; only the test project may use NuGet packages.
4. Follow the existing structure (see *Architecture* in the [README](README.md)):
   - a new CLI verb = a new `ICommand` class + one line in `CompositionRoot`;
   - new list filter = a new `TaskFilter` instance;
   - new storage = a new `ITaskRepository` implementation.
5. Add tests for new behaviour (unit tests plus an end-to-end case in `CliEndToEndTests`).

Open an issue first for larger changes.
