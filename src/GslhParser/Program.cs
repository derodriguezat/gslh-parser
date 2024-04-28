using System.CommandLine;
using System.CommandLine.Parsing;
using GslhParser;

// Define the root command.
var rootCommand = new RootCommand("Google Semantic Location History Parser");

// Path argument.
var pathArgument = new Argument<string>(
    name: "path",
    description: "The path to the SLH",
    parse: (parseArgument) =>
{
    if (!Directory.Exists(parseArgument.Tokens[0].Value))
    {
        parseArgument.ErrorMessage = "The path is invalid. Please try again.";
        return default;
    }

    return parseArgument.Tokens[0].Value;
})
{
    Arity = ArgumentArity.ExactlyOne,
};

rootCommand.AddArgument(pathArgument);

// Workdays command.
var workdaysCommand = new Command("workdays", "Get the number of workdays in a year and month");
rootCommand.AddCommand(workdaysCommand);

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

await rootCommand.InvokeAsync(args);