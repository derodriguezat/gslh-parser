using GslhParser;

Console.WriteLine("Hello, This is the Semantic Location History Parser!");
Console.WriteLine("Please enter the path to the SLH");

// Read the path to the SLH and check if it is valid
var pathToSlh = Console.ReadLine();
if (!Directory.Exists(pathToSlh))
{
    Console.WriteLine("The path is invalid. Please try again.");
    return;
}

var slhRoot = new SlhRoot(pathToSlh);
Console.WriteLine($"The following years have been found in your SLH:\n {string.Join(',', slhRoot.GetSlhYears().Select(y => y.Year))}.");

// Read the year from the user
Console.WriteLine("Please enter the year you want to analyze.");
var year = Console.ReadLine();
if (!int.TryParse(year, out var yearInt))
{
    Console.WriteLine("The year is invalid. Please try again.");
    return;
}

if (!slhRoot.TryGetYear(yearInt, out var slhYear))
{
    Console.WriteLine($"Year {yearInt} not found.");
    return;
}

Console.WriteLine($"The following months have been found in your SLH for the year {yearInt}:\n {string.Join(',', slhYear!.GetSlhMonths().OrderBy(m => m.Month).Select(m => m.Month))}.");

// Read the month from the user
Console.WriteLine("Please enter the num ber of month you want to analyze.");
var month = Console.ReadLine();
if (!int.TryParse(month, out var monthInt))
{
    Console.WriteLine("The month is invalid. Please try again.");
    return;
}

if (!slhYear.TryGetMonth(monthInt, out var slhMonth))
{
    Console.WriteLine($"Month {monthInt} not found.");
    return;
}

var workVisits = slhMonth!.GetWorkVisits();
Console.WriteLine($"Work days on year {yearInt} : {workVisits.Count()}");
Console.ReadLine();