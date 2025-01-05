using System.CommandLine;
using System.CommandLine.Parsing;
using GslhParser;

// Define the root command.
var rootCommand = new RootCommand("Google Semantic Location History Parser");
AddFromDirCommand(rootCommand);
AddFromFileCommand(rootCommand);

await rootCommand.InvokeAsync(args);

static void AddFromFileCommand(RootCommand rootCommand)
{
    var fromFileCommand = new Command("from-file", "Parse the SLH from a file");
    rootCommand.AddCommand(fromFileCommand);

    var fileArgument = new Argument<string>(
        name: "file",
        description: "The file containing the SLH",
        parse: (parseArgument) =>
        {
            if (!File.Exists(parseArgument.Tokens[0].Value))
            {
                parseArgument.ErrorMessage = "The file does not exist. Please try again.";
                return default;
            }

            return parseArgument.Tokens[0].Value;
        })
    {
        Arity = ArgumentArity.ExactlyOne
    };

    fromFileCommand.AddArgument(fileArgument);

    // Workdays command.
    var workdaysCommand = new Command("workdays", "Get the number of workdays in a year and month");
    fromFileCommand.AddCommand(workdaysCommand);

    // Year options.
    var yearOption = new Argument<int>(
        name: "year",
        description: "The year you want to analyze",
        parse: (argResult) =>
        {
            if (!int.TryParse(argResult.Tokens[0].Value, out var year) || year < 2010 || year > DateTime.Now.Year)
            {
                argResult.ErrorMessage = $"The year is invalid. It should be a value between 2010 and {DateTime.Now.Year}";
                return default;
            }

            return year;
        })
    {
        Arity = ArgumentArity.ExactlyOne
    };

    // Month option.
    var monthOption = new Option<int>(
        name: "--month",
        description: "The month you want to analyze", parseArgument: (argResult) =>
        {
            if (!int.TryParse(argResult.Tokens[0].Value, out var month) || month < 1 || month > 12)
            {
                argResult.ErrorMessage = "The month is invalid. It should be a value between 1 and 12";
                return default;
            }

            return month;
        })
    {
        Arity = ArgumentArity.ZeroOrOne
    };

    workdaysCommand.AddArgument(yearOption);
    workdaysCommand.AddOption(monthOption);
    workdaysCommand.SetHandler((path, year, month) =>
    {
        using var stream = File.OpenRead(path);
        var jsonDocument = System.Text.Json.JsonDocument.Parse(stream);
        if (!jsonDocument.RootElement.TryGetProperty("semanticSegments", out var semanticSegments))
        {
            Console.WriteLine("The specified file does not contain any semantic segments!");
            return;
        }

        var workVisits = new HashSet<DateTime>();
        foreach (var semanticSegment in semanticSegments.EnumerateArray())
        {
            if (!semanticSegment.TryGetProperty("startTime", out var startTimeProperty) ||
                !startTimeProperty.TryGetDateTime(out var startTime))
            {
                continue;
            }

            if (startTime.Year != year ||
                month > 0 && startTime.Month != month)
            {
                continue;
            }

            if (!semanticSegment.TryGetProperty("endTime", out var endTimeProperty) ||
                !startTimeProperty.TryGetDateTime(out var endTime))
            {
                continue;
            }

            if (!semanticSegment.TryGetProperty("visit", out var visit) ||
                !visit.TryGetProperty("topCandidate", out var topCandidate) ||
                !topCandidate.TryGetProperty("semanticType", out var semanticType) ||
                semanticType.GetString() != "WORK")
            {
                continue;
            }

            workVisits.Add(startTime.Date);
        }

        Console.WriteLine($"Workdays {year}");
        Console.WriteLine(string.Join("\n", workVisits.GroupBy(d => d.Month).Select(g => $"{g.Key}\t{g.Count()}")));
        Console.WriteLine($"Total\t{workVisits.Count}");

    }, fileArgument, yearOption, monthOption);
}

static void AddFromDirCommand(RootCommand rootCommand)
{
    var fromDirCommand = new Command("from-dir", "Parse the SLH from a directory");
    rootCommand.AddCommand(fromDirCommand);

    // Path argument.
    var pathArgument = new Argument<string>(
        name: "path",
        description: "The path to the SLH",
        parse: (parseArgument) =>
    {
        if (!Directory.Exists(parseArgument.Tokens[0].Value))
        {
            parseArgument.ErrorMessage = "The path is should be a directoty containing the SLH. Please try again.";
            return default;
        }

        return parseArgument.Tokens[0].Value;
    })
    {
        Arity = ArgumentArity.ExactlyOne,
    };

    fromDirCommand.AddArgument(pathArgument);

    // Workdays command.
    var workdaysCommand = new Command("workdays", "Get the number of workdays in a year and month");
    fromDirCommand.AddCommand(workdaysCommand);

    // Year options.
    var yearOption = new Argument<int>(
        name: "year",
        description: "The year you want to analyze",
        parse: (argResult) =>
        {
            if (!int.TryParse(argResult.Tokens[0].Value, out var year) || year < 2010 || year > DateTime.Now.Year)
            {
                argResult.ErrorMessage = $"The year is invalid. It should be a value between 2010 and {DateTime.Now.Year}";
                return default;
            }

            return year;
        })
    {
        Arity = ArgumentArity.ExactlyOne
    };

    // Month option.
    var monthOption = new Option<int>(
        name: "--month",
        description: "The month you want to analyze", parseArgument: (argResult) =>
        {
            if (!int.TryParse(argResult.Tokens[0].Value, out var month) || month < 1 || month > 12)
            {
                argResult.ErrorMessage = "The month is invalid. It should be a value between 1 and 12";
                return default;
            }

            return month;
        })
    {
        Arity = ArgumentArity.ZeroOrOne
    };

    workdaysCommand.AddArgument(yearOption);
    workdaysCommand.AddOption(monthOption);
    workdaysCommand.SetHandler((path, year, month) =>
    {
        var slhRoot = new SlhRoot(path);
        if (!slhRoot.TryGetYear(year, out var slhYear))
        {
            Console.WriteLine($"Year {year} was not found.");
            return;
        }

        if (month != 0)
        {
            if (!slhYear!.TryGetMonth(month, out var slhMonth))
            {
                Console.WriteLine($"Month {month} not found.");
                return;
            }

            Console.WriteLine($"The number of workdays in {year}-{month} was {slhMonth!.GetWorkVisits().Count()}");
            return;
        }

        var workVisits = slhYear!.GetSlhMonths().SelectMany(m => m.GetWorkVisits());
        Console.WriteLine($"The number of workdays in {year} is {workVisits.Count()}");
    }, pathArgument, yearOption, monthOption);
}