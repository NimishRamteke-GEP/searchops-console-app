using System;
using TrialAndErrorConsoleApp;

namespace ElasticSearchIndexManagement
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await RunApplication();
            }
            catch (Exception ex)
            {
                Logger.LogMessage(LogLevel.ERROR, $"Exception occured {ex.Message} | {ex}");
                await RunApplication();
            }
        }

        static async Task RunApplication()
        {
            int i = 100;
            while (i != 9)
            {
                Logger.LogMessage(LogLevel.INFO, "Select operation ny entering number");
                Logger.LogMessage(LogLevel.INFO, "1. Update Template");
                Logger.LogMessage(LogLevel.INFO, "2. Rename fields");
                Logger.LogMessage(LogLevel.ERROR, "9. Exit!");
                i = Convert.ToInt32(Console.ReadLine());
                Logger.LogMessage(LogLevel.INFO, $"User Input: {i}");
                switch (i)
                {
                    case 1:
                        await UpdateIndexTemplate();
                        break;
                    case 2:
                        await ExecuteRenamaeScript();
                        break;
                    case 9:
                        Logger.LogMessage(LogLevel.INFO, "Exiting!");
                        break;
                    default:
                        Logger.LogMessage(LogLevel.WARN, "Invalid Input. Please try again");
                        break;
                }
            }
            Logger.LogMessage(LogLevel.INFO, "Exited!");
        }

        static async Task UpdateIndexTemplate()
        {
            Logger.PrintDivider();

            Logger.LogMessage(LogLevel.INFO, "Enter the environment (DEV, QC ,UATUS, UATEU, UATAPAC, UATSEA, PRODUS , PRODEU, PRODAPAC, PRODSEA):");
            string environment = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {environment}");

            if (!Enum.TryParse<ElasticEnvironment>(environment, true, out ElasticEnvironment env))
            {
                Logger.LogMessage(LogLevel.ERROR, "Invalid environment");
            }

            Logger.LogMessage(LogLevel.INFO, "Executing UpdateIndex Template");
            UpdateIndexTemplateHelper updateIndexTemplateHelper = new UpdateIndexTemplateHelper();

            var indexList = new List<string>
            {
            "dm-idx-nimish-v003"

            };


            indexList = indexList.Distinct().ToList();
            indexList.OrderBy(x => x).ToList();
            indexList = indexList.Where(s => !s.Contains("searchops")).ToList();

            Logger.LogMessage(LogLevel.INFO, $"Total indices: {indexList.Count}");
            string version = "1.2";
            await updateIndexTemplateHelper.UpdateIndexTemplate(env, indexList, version);
            Logger.PrintDivider();
        }

        static async Task ExecuteRenamaeScript()
        {
            Logger.PrintDivider();

            Logger.LogMessage(LogLevel.INFO, "Enter the environment (DEV, QC ,UATUS, UATEU, UATAPAC, UATSEA, PRODUS , PRODEU, PRODAPAC, PRODSEA):");
            string environment = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {environment}");

            if (!Enum.TryParse<ElasticEnvironment>(environment, true, out ElasticEnvironment env))
            {
                Logger.LogMessage(LogLevel.ERROR, "Invalid environment");
            }

            Logger.LogMessage(LogLevel.INFO, "Enter index name");
            var indexName = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {indexName}");

            Logger.LogMessage(LogLevel.INFO, "Enter ticket id");
            var ticketId = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {indexName}");


            await ScriptExecuteHelper.ExecuteRenameScript(env, indexName, ticketId);
            Logger.PrintDivider();
        }
    }
}