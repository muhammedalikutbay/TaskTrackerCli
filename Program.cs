using TaskTrackerCli.Services;

var jsonFileService = new JsonFileService();
var taskService = new TaskService(jsonFileService);

if (args.Length == 0)
{
    Console.WriteLine("Please provide a command.");
    return;
}

var command = args[0];

switch (command.ToLower())
{
    case "add":
        if (args.Length < 2)
        {
            Console.WriteLine("Please provide a task description.");
            return;
        }

        var description = args[1];
        var task = taskService.AddTask(description);

        Console.WriteLine($"Task added successfully (ID: {task.Id})");
        break;

    default:
        Console.WriteLine("Unknown command.");
        break;
}