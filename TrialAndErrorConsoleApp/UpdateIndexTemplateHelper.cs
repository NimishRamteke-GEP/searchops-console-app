using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TrialAndErrorConsoleApp
{
    class UpdateIndexTemplateHelper
    {
        public async Task UpdateIndexTemplate(ElasticEnvironment environment, List<string> indexNames, string version)
        {
            IndexManager _indexManager = new IndexManager(environment);

            Logger.LogMessage(LogLevel.INFO, $"Script will executed for following indices on Environment: {environment.ToString()}");

            foreach (var indexName in indexNames)
            {
                Logger.LogMessage(LogLevel.INFO, indexName);
            }
            Logger.LogMessage(LogLevel.INFO, $"Update to version for template is: {version}");
            Logger.LogMessage(LogLevel.INFO, "Enter confirm to proceed");
            string confirmationText = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {confirmationText}");

            if (!confirmationText.Equals("confirm"))
            {
                Logger.LogMessage(LogLevel.ERROR, "Script aborted! Failed to confirm action.");
                throw new Exception("Script aborted!");
            }
            Logger.PrintDivider();
            try
            {
                foreach (var indexName in indexNames)
                {
                    var currentVersion = await _indexManager.GetCurrentIndexTemplateVersion(indexName);
                    if (currentVersion.Equals("0"))
                    {
                        Logger.LogMessage(LogLevel.ERROR, $"Version is not present for current index: {indexName}. Hence skipping reindexing.");
                        continue;
                        throw new Exception("Version is not present for current Index");
                    }

                    if (currentVersion.Equals(version))
                    {
                        Logger.LogMessage(LogLevel.WARN, $"Index: {indexName} has current version: {currentVersion} which is equal to update version: {version}. Hence skiping the reindexing.");
                        continue;
                    }

                    Logger.LogMessage(LogLevel.WARN, $"Index: {indexName} has current version: {currentVersion} which is not equal to update version: {version}. Hence doing the reindexing.");


                    var docCount = await _indexManager.GetDocumentCount(indexName);

                    Logger.PrintDivider();
                    Logger.LogMessage(LogLevel.INFO, $"Executing script for index: {indexName}");

                    string backupIndexName = $"{indexName}3414";
                    if (docCount == 0)
                    {
                        Logger.LogMessage(LogLevel.WARN, $"Skipping reindexing as document count is {docCount}");
                    }
                    else
                    {
                        Logger.LogMessage(LogLevel.WARN, $"Doing reindexing as document count is {docCount}");
                    }
                    //Create backup index
                    if (docCount > 0)
                    {
                        await _indexManager.CreateIndex(backupIndexName, "");

                        Logger.PrintDivider();
                        Logger.LogMessage(LogLevel.INFO, $"Created backup index: {backupIndexName}");
                        Logger.PrintDivider();

                        await _indexManager.ReindexData(indexName, backupIndexName);
                        Logger.PrintDivider();
                    }

                    var indexAliasMetaBody = await _indexManager.GetAliasesAndMeta(indexName);
                    indexAliasMetaBody.SelectToken(indexName).SelectToken("mappings").SelectToken("_meta").SelectToken("esMetadata").Parent.Remove();

                    await _indexManager.DeleteIndex(indexName);

                    Logger.PrintDivider();

                    await _indexManager.CreateIndex(indexName, indexAliasMetaBody[indexName].ToString());
                    Logger.PrintDivider();

                    if (docCount > 0)
                    {
                        await _indexManager.ReindexData(backupIndexName, indexName);
                        await _indexManager.DeleteIndex(backupIndexName);
                    }

                    Logger.PrintDivider();
                    Logger.LogMessage(LogLevel.INFO, $"Script Ended");
                   // Console.ReadLine();
                    Logger.PrintDivider();
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(LogLevel.ERROR, $"Exception occured while updating template: {ex.Message}");
                throw;
            }
        }
    }
}
