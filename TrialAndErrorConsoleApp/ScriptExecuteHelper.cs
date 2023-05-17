using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrialAndErrorConsoleApp
{
    public class ScriptExecuteHelper
    {

        public static async Task ExecuteRenameScript(ElasticEnvironment environment, string indexName, string ticketId)
        {
            Logger.LogMessage(LogLevel.INFO, $"Executing Execute Renamae Script for env {environment.ToString()}, IndexName: {indexName}, TicketId {ticketId}");

            Logger.LogMessage(LogLevel.INFO, "Enter confirm to proceed");
            string confirmationText = Console.ReadLine().ToString();
            Logger.LogMessage(LogLevel.INFO, $"User Input: {confirmationText}");

            if (!confirmationText.Equals("confirm"))
            {
                Logger.LogMessage(LogLevel.ERROR, "Script aborted! Failed to confirm action.");
                throw new Exception("Script aborted!");
            }

            IndexManager _indexManager = new IndexManager(environment);

            await _indexManager.TakeSnapShot(indexName, ticketId);
            Logger.PrintDivider();
            long tick = DateTime.Now.Ticks;
            string backupIndexName = $"{indexName}_automation_{tick}{ticketId}";
            Logger.LogMessage(LogLevel.INFO, $"Creating backup index: {backupIndexName}");

            await _indexManager.CreateIndex(backupIndexName, "");
            Logger.PrintDivider();

            await _indexManager.ReindexData(indexName, backupIndexName);
            Logger.PrintDivider();

            string scriptFilePath = $@"C:\Users\nimish.ramteke\source\repos\TrialAndErrorConsoleApp\TrialAndErrorConsoleApp\Scripts\{ticketId}.txt";
            string script = File.ReadAllText(scriptFilePath);

            Logger.PrintDivider();

            var indexAliasMetaBody = await _indexManager.GetAliasesAndMeta(indexName);
            indexAliasMetaBody.SelectToken(indexName).SelectToken("mappings").SelectToken("_meta").SelectToken("esMetadata").Parent.Remove();

            await _indexManager.DeleteIndex(indexName);

            Logger.PrintDivider();

            await _indexManager.CreateIndex(indexName, indexAliasMetaBody[indexName].ToString());
            Logger.PrintDivider();

            Logger.LogMessage(LogLevel.INFO, $"Executing Rename script for index: {indexName}");

            await _indexManager.ReindexData(backupIndexName, indexName, script);
            
            Logger.LogMessage(LogLevel.INFO, $"Rename script is excuted for index: {indexName}");
            Logger.PrintDivider();


        }
    }
}
