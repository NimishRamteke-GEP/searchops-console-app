namespace TrialAndErrorConsoleApp
{
    static class ElasticHelper
    {
        private static Dictionary<ElasticEnvironment, ElasticsearchCredential> _credentials = new Dictionary<ElasticEnvironment, ElasticsearchCredential>
        {
            {
                ElasticEnvironment.DEV, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.QC, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.UATUS, new ElasticsearchCredential
                {
                     Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.UATEU, new ElasticsearchCredential
                {
                     Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.UATAPAC, new ElasticsearchCredential
                {
                   Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.UATSEA, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.PRODUS, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.PRODEU, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.PRODAPAC, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
            {
                ElasticEnvironment.PRODSEA, new ElasticsearchCredential
                {
                    Repository = "backuprepos",
                    Username = "qc_user",
                    Password = "qc_user"
                }
            },
        };

        public static ElasticsearchCredential GetCredentials(ElasticEnvironment environment)
        {
            if (!_credentials.TryGetValue(environment, out var credentials))
            {
                throw new ArgumentException($"No credentials found for environment {environment}");
            }

            return credentials;
        }
    }
}