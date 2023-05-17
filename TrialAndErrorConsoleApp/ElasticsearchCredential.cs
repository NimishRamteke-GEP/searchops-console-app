namespace TrialAndErrorConsoleApp
{
    public class ElasticsearchCredential
    {
        public ElasticEnvironment Enviroment { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Repository { get; set; }
    }
    public enum ElasticEnvironment
    {
        DEV = 1,
        QC = 2,
        UATUS = 3,
        UATEU = 4,
        UATAPAC = 5,
        UATSEA = 6,
        PRODUS = 7,
        PRODEU = 8,
        PRODAPAC = 9,
        PRODSEA = 10
    }
}
