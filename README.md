# Task Tracker CLI

A small command line app for tracking what you need to do, what you are working on, and what you have finished.
Tasks are stored in a `tasks.json` file in the directory you run the command from (created automatically).

Built with **C# / .NET 10** and **no third-party libraries** – only the base class library (`System.Text.Json`, `System.IO`, ...).
The test project uses xUnit, but the app itself has zero dependencies.

## Getting started

```bash
# run directly from source
dotnet run --project src/TaskTrackerCli -- add "Buy groceries"

# or publish a build and use it as `task-cli`
dotnet publish src/TaskTrackerCli -c Release -o ./publish
./publish/task-cli list          # Windows: publish\task-cli.exe list
```

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

## Usage

```bash
# Adding, updating and deleting tasks
task-cli add "Buy groceries"            # Task added successfully (ID: 1)
task-cli update 1 "Buy groceries and cook dinner"
task-cli delete 1

# Changing status
task-cli mark-in-progress 1
task-cli mark-done 1
task-cli mark-todo 1                    # bonus: put a task back to todo

# Listing
task-cli list                           # all tasks
task-cli list todo
task-cli list in-progress
task-cli list done
task-cli list not-done                  # todo + in-progress

task-cli help
```

Example output:

```
ID  STATUS       CREATED           UPDATED           DESCRIPTION
1   done         2026-11-02 10:24  2026-11-02 10:24  Buy groceries
2   in-progress  2026-11-02 10:24  2026-11-02 10:30  Write quarterly report
```

Quote descriptions that contain special shell characters. Plain words also work (`add Buy milk` stores "Buy milk").

### Exit codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | The command could not be carried out (task not found, unreadable/corrupt file, empty description) |
| 2 | Wrong command line (unknown command, missing/invalid arguments) |

Errors are written to stderr, results to stdout.

## Storage format

`tasks.json` is a JSON array:

```json
[
  {
    "id": 1,
    "description": "Buy groceries",
    "status": "in-progress",
    "createdAt": "2026-11-02T10:24:33+00:00",
    "updatedAt": "2026-11-02T10:30:01+00:00"
  }
]
```

`status` is one of `todo`, `in-progress`, `done`. Timestamps are ISO 8601 (UTC).

Robustness notes:

* A missing or empty file is treated as "no tasks"; a missing file is created.
* A file that is not valid (bad JSON, unknown status, duplicate/invalid ids, missing fields) is reported with a clear
  message and **never overwritten**, so you cannot lose data by running a command.
* Saves are atomic (write to a temp file, then replace), so a crash cannot leave a half-written file.
* IDs are `max(existing id) + 1`.

## Architecture

```
src/TaskTrackerCli
├── Domain/            TaskItem, TodoStatus, TaskFilter, exceptions    (no dependencies)
├── Application/       TaskService + ports: ITaskRepository, IClock,
│                      ITaskQueryService / ITaskCommandService
├── Infrastructure/    JsonFileTaskRepository, SystemClock, JSON converter
├── Cli/               ICommand, CommandRegistry, CliApplication,
│   └── Commands/      Add / Update / Delete / MarkStatus / List
└── Program.cs         three lines; CompositionRoot wires everything
tests/TaskTrackerCli.Tests   unit tests + end-to-end tests of the whole CLI
```

How the SOLID principles and patterns show up:

* **Single responsibility** – each class has one job: `TaskService` (use cases), `JsonFileTaskRepository` (persistence),
  `TaskTableFormatter` (presentation), `ArgumentParser` (input parsing), `CliApplication` (dispatch + error → exit code),
  `HelpPrinter` (help text).
* **Open/closed** – a new verb is a new `ICommand` class plus one line in `CompositionRoot`; a new list filter is one
  more `TaskFilter` instance. The dispatcher, help text and parser do not change.
* **Liskov** – every `ICommand` is interchangeable behind the registry; every `ITaskRepository` (JSON file, in-memory
  fake in tests) honours the same contract.
* **Interface segregation** – commands that only read depend on `ITaskQueryService`, commands that write on
  `ITaskCommandService`; neither sees the other's methods.
* **Dependency inversion** – the application layer depends on `ITaskRepository` and `IClock`; the concrete JSON/system
  implementations are chosen only in `CompositionRoot` (manual constructor injection).
* **Patterns** – Command (`ICommand`), Repository (`ITaskRepository`), Registry (`CommandRegistry`), Strategy
  (`TaskFilter` predicates), Factory method (`TaskItem.Create`, `MarkStatusCommand.Done(...)`), immutable value
  objects (`TaskItem` is a record; changes return a new instance).

## Tests

```bash
dotnet test
```

Unit tests cover the domain, service (with an in-memory repository and fake clock), the JSON repository (real temp
files, corrupt-file handling) and the formatter. End-to-end tests run the full CLI against a temp `tasks.json`.
