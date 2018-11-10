using NodaTime;
using PowerArgs;

namespace DadsEnergyReporter.Entry
{
    public class Options
    {
//        [Option('u', "skip-utility", Required = false, Default = false,
//            HelpText = "true to only include solar data in the report, " +
//                       "false to generate a report with both utility and solar data")]
        [ArgDefaultValue(false)]
        public bool SkipUtility { get; set; }

//        [Option('o', "output", Required = false, Default = ReportDestination.Email,
//            HelpText = "Email to send the report by email to the configured recipients," +
//                       "ConsoleJson to write the report to the console in JSON format")]
//        public ReportDestination OutputDestination { get; set; }

//        [Option('s', "start-date", Required = false, HelpText = "The first day of the billing cycle (inclusive)")]
        public LocalDate StartDate { get; set; }

//        [Option('e', "end-date", Required = false, HelpText = "The last day of the billing cycle (inclusive)")]
        public LocalDate EndDate { get; set; }

//        public enum ReportDestination
//        {
//            Email,
//            ConsoleJson
//        }
    }
}