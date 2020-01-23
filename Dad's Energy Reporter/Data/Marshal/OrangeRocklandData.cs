namespace DadsEnergyReporter.Data.Marshal {

    public class OrangeRocklandAuthToken {

        public string logInCookie { get; set; }

        public override string ToString() {
            return $"{nameof(logInCookie)}: {logInCookie}";
        }

    }

}