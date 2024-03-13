using ExaPlusPlus.Language;
using System.Reflection.Emit;


// if flag 'w' is present in args
void CompileFile(string s)
{
    var levelName = Path.GetFileNameWithoutExtension(s);
    var compiledFilename = levelName + ".exppc";
    var levelData = File.ReadAllText(s);

    var parser = new Parser();
    var result = parser.Parse(levelData);
    var compiler = new Compiler();
    result.Accept(compiler);
    var compiled = compiler.Compilation.ToString();
    File.WriteAllText(compiledFilename, compiled);
}

if (args.Length==0)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("\r ExaPlusPlus.exe --w <dir>");
    Console.WriteLine("\r ExaPlusPlus.exe <filename>");
    return;
}

if (args[0]!="--w")
{
    var filepath = args[0];
    try
    {
        CompileFile(filepath);
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error while compiling:");
        Console.ResetColor();
        Console.WriteLine(e);
        throw;
    }
    
}
else
{
    var dir = args[1];
    // convert to absolute path
    dir = Path.GetFullPath(dir);
    Console.WriteLine($"Watching {dir} for changes");
  

    // watch files in folder
    var watcher = new FileSystemWatcher();
    watcher.Path = dir;
    watcher.Filter = "*.expp";
    watcher.NotifyFilter= NotifyFilters.LastWrite;
    watcher.EnableRaisingEvents = true;
    watcher.Changed += (sender, eventArgs) =>
    {
        Console.WriteLine($"File {eventArgs.FullPath} changed");
        Thread.Sleep(100);
        // wait till file is not busy anymore
        while (true)
        {
            try
            {
                using (var stream = File.Open(eventArgs.FullPath, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    break;
                }
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
        }

        try
        {
            CompileFile(eventArgs.FullPath);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error while compiling:");
            Console.ResetColor();
            Console.WriteLine(e);
        }
        
        
    };

    // wait for termination
    var exitEvent = new ManualResetEvent(false);
    Console.CancelKeyPress += (sender, eventArgs) =>
    {
        eventArgs.Cancel = true;
        exitEvent.Set();
    };
    exitEvent.WaitOne();
}




