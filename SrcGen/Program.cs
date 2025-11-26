using SrcGen;

if (args.Length < 4)
{
    Console.WriteLine("""
        Need 4 arguments:

        0. Path to the dbgeng.h file.
        1. Path to the missing.h file.
        2. Path to the generated file.
        3. Path to the dir containing the document source files.
        """);
    return -1;
}

Console.WriteLine($"SrcGen is running at {Environment.CurrentDirectory}");

using var hpp = File.OpenText(args[0]);
using var missing = File.Exists(args[1]) ? File.OpenText(args[1]) : StreamReader.Null;
using var output = new StreamWriter(new FileStream(args[2], FileMode.Create));

var documents = Documents.From(args[3]);
var translator = new Translator(output, documents);

translator.Generate(hpp, missing);

return 0;
