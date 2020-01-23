using NodaTime;

namespace DadsEnergyReporter.Data.Marshal {

    public class TeslaAuthToken {

        public string accessToken;
        public string tokenType;
        public Instant expiration;
        public Instant creation;

    }

}