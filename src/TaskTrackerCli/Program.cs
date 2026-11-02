using TaskTrackerCli.Cli;

var tasksFile = Path.Combine(Directory.GetCurrentDirectory(), CompositionRoot.DefaultFileName);
return CompositionRoot.Create(tasksFile, new ConsoleOutput()).Run(args);
