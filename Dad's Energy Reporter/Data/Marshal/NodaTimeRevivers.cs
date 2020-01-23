using System;
using NodaTime;
using NodaTime.Text;
using PowerArgs;

namespace DadsEnergyReporter.Data.Marshal {

    public class NodaTimeRevivers {

        public LocalDate localDate { get; set; }

        [ArgReviver]
        public static LocalDate revive(string key, string value) {
            try {
                return LocalDatePattern.Iso.Parse(value).GetValueOrThrow();
            } catch (Exception e) {
                throw new ArgException($"Failed to parse command-line argument {key} from {value} to a LocalDate", e);
            }
        }

    }

}