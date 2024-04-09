namespace MyGarage;

public class AppConfiguration
{
    private string _theGarageHost = "http://the-garage.example.com:8080";

    public string TheGarageHost
    {
        get
        {
            var hostFromEnv = Environment.GetEnvironmentVariable("THE_GARAGE_HOST");
            return hostFromEnv ?? _theGarageHost;
        }

        set => _theGarageHost = value;
    }
}
