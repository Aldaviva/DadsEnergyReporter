namespace DadsEnergyReporter.Data.Marshal
{
    public class OrangeRocklandAuthToken
    {
        public string LogInCookie { get; set; }

        public override string ToString()
        {
            return $"{nameof(LogInCookie)}: {LogInCookie}";
        }
    }
}