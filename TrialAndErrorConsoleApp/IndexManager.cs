using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TrialAndErrorConsoleApp
{
    public class IndexManager
    {
        ElasticsearchCredential elasticsearchCredential = new ElasticsearchCredential();
        HttpClient client;

        public IndexManager(ElasticEnvironment environment)
        {
            elasticsearchCredential = ElasticHelper.GetCredentials(environment);

            client = new HttpClient();
            string authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{elasticsearchCredential.Username}:{elasticsearchCredential.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
        }

        public async Task CreateIndex(string indexName, string indexBody)
        {
            Logger.LogMessage(LogLevel.INFO, $"Creating Index: {indexName}");
            string createIndexUrl = $"{elasticsearchCredential.Url}/{indexName}";
            HttpResponseMessage creatIndexResponse = await client.PutAsync(createIndexUrl, new StringContent(indexBody, Encoding.UTF8, "application/json"));
            creatIndexResponse.EnsureSuccessStatusCode();
            Logger.LogMessage(LogLevel.INFO, $"Created Index: {indexName}");
        }

        public async Task ReindexData(string sourceIndex, string destinationIndex, string script = "")
        {
            // Reindex data to backup index
            string reindexUrl = $"{elasticsearchCredential.Url}/_reindex?wait_for_completion=false";
            string reindexPayload = "{\"source\": {\"index\": \"" + sourceIndex + "\"}, \"dest\": {\"index\": \"" + destinationIndex + "\"}}";
            
            if (!string.IsNullOrEmpty(script))
            {
                reindexPayload = "{\"source\": {\"index\": \"" + sourceIndex + "\"}, \"dest\": {\"index\": \"" + destinationIndex + "\"}, \"script\": " + script + "}";
                
                //reindexPayload = "{\"source\":{\"index\":\""+ sourceIndex + " \",\"_source\":{\"excludes\":[\"arr$componentmaterial.obj$componentitem.kwd$description\",\"arr$componentmaterial.obj$componentitem.kwd$itemnumber\",\"arr$componentmaterial.obj$componentitem.kwd$name\",\"arr$notes.obj$createdby.kwd$firstname\",\"arr$notes.obj$createdby.kwd$lastname\",\"arr$notes.obj$modifiedby.kwd$firstname\",\"arr$notes.obj$modifiedby.kwd$lastname\",\"arr$orderitems.arr$notes.obj$createdby.kwd$emailaddress\",\"arr$orderitems.arr$notes.obj$createdby.kwd$firstname\",\"arr$orderitems.arr$notes.obj$createdby.kwd$lastname\",\"arr$orderitems.arr$notes.obj$createdby.kwd$username\",\"arr$orderitems.arr$notes.obj$createdby.lng$contactcode\",\"arr$orderitems.arr$notes.obj$modifiedby.kwd$emailaddress\",\"arr$orderitems.arr$notes.obj$modifiedby.kwd$firstname\",\"arr$orderitems.arr$notes.obj$modifiedby.kwd$lastname\",\"arr$orderitems.arr$notes.obj$modifiedby.kwd$username\",\"arr$orderitems.arr$notes.obj$modifiedby.lng$contactcode\",\"arr$orderitems.kwd$itemtypeindicator\",\"arr$orderitems.kwd$migrationsource\",\"arr$orderitems.obj$changetype.kwd$clientcode\",\"arr$orderitems.obj$changetype.kwd$code\",\"arr$orderitems.obj$changetype.kwd$description\",\"arr$orderitems.obj$changetype.kwd$name\",\"arr$orderitems.obj$changetype.lng$appcodeid\",\"arr$orderitems.obj$operationtype.kwd$code\",\"arr$orderitems.obj$operationtype.kwd$description\",\"arr$orderitems.obj$operationtype.kwd$name\",\"arr$orderitems.obj$operationtype.lng$appcodeid\",\"obj$attachments.obj$createdby.kwd$emailaddress\",\"obj$attachments.obj$createdby.kwd$firstname\",\"obj$attachments.obj$createdby.kwd$lastname\",\"obj$attachments.obj$createdby.kwd$username\",\"obj$attachments.obj$createdby.lng$contactcode\",\"obj$changetype.kwd$codetype\",\"obj$changetype.kwd$description\",\"obj$changetype.kwd$id\",\"obj$changetype.kwd$name\",\"obj$changetype.kwd$source\",\"obj$changetype.kwd$status\",\"obj$changetype.lng$appcodeid\",\"obj$operationtype.kwd$clientcode\",\"obj$operationtype.kwd$code\",\"obj$operationtype.kwd$codetype\",\"obj$operationtype.kwd$description\",\"obj$operationtype.kwd$id\",\"obj$operationtype.kwd$name\",\"obj$operationtype.kwd$source\",\"obj$operationtype.kwd$status\",\"obj$operationtype.lng$appcodeid\",\"obj$orderpriority.kwd$clientcode\",\"obj$orderpriority.kwd$code\",\"obj$orderpriority.kwd$codetype\",\"obj$orderpriority.kwd$description\",\"obj$orderpriority.kwd$id\",\"obj$orderpriority.kwd$name\",\"obj$orderpriority.kwd$source\",\"obj$orderpriority.kwd$status\",\"obj$orderpriority.lng$appcodeid\"]}},\"dest\":{\"index\":\"" + destinationIndex + "\"}}";

                Logger.LogMessage(LogLevel.INFO, $"Reindexing script is: {reindexPayload}. Will be executed on the environment: {elasticsearchCredential.Enviroment.ToString()}. Enter confirm to proceed");
                string confirmationText = Console.ReadLine().ToString();
                Logger.LogMessage(LogLevel.INFO, $"User Input: {confirmationText}");

                if (!confirmationText.Equals("confirm"))
                {
                    Logger.LogMessage(LogLevel.ERROR, "Script aborted! Failed to confirm action.");
                    throw new Exception("Script aborted!");
                }
                Logger.LogMessage(LogLevel.INFO, "Continuing the execution");
            }
            

            Logger.LogMessage(LogLevel.INFO, $"Reindexing data from {sourceIndex} to {destinationIndex}");
            //var p = JsonConvert.DeserializeObject(reindexPayload);

            HttpResponseMessage reindexResponse = await client.PostAsync(reindexUrl, new StringContent(reindexPayload, Encoding.UTF8, "application/json"));

           

            var reindexTaskResponse = JObject.Parse(await reindexResponse.Content.ReadAsStringAsync());
            reindexResponse.EnsureSuccessStatusCode();
            bool isReindexComplete = false;
            string taskId = reindexTaskResponse["task"].ToString();
            var tasksUrl = $"{elasticsearchCredential.Url}/_tasks/{taskId}";

            Logger.LogMessage(LogLevel.INFO, $"Straing Reindexing from {sourceIndex} -> {destinationIndex}. TaskId: {taskId}");

            while (!isReindexComplete)
            {
                HttpResponseMessage tasksResponse = await client.GetAsync(tasksUrl);
                tasksResponse.EnsureSuccessStatusCode();
                var reindexTaskStatusResponse = JObject.Parse(await tasksResponse.Content.ReadAsStringAsync());
                if (reindexTaskStatusResponse != null && reindexTaskStatusResponse["completed"].ToString() == "True")
                {
                    if (reindexTaskStatusResponse["response"]["failures"].HasValues)
                    {
                        throw new Exception($"Failed while reindexing. Failure: {reindexTaskStatusResponse["response"]["failures"].ToString()}");
                    }
                    
                    isReindexComplete = true;
                }

                Logger.LogMessage(LogLevel.INFO, $"Reindexing from {sourceIndex} -> {destinationIndex}");
                Logger.LogMessage(LogLevel.WARN, $"Total {reindexTaskStatusResponse["task"]["status"]["total"]}");
                Logger.LogMessage(LogLevel.WARN, $"Created {reindexTaskStatusResponse["task"]["status"]["created"]}");
                Logger.LogMessage(LogLevel.WARN, $"Updated {reindexTaskStatusResponse["task"]["status"]["updated"]}");
                Logger.PrintDivider();

                Thread.Sleep(1000);
            }
            Logger.LogMessage(LogLevel.WARN, $"Reindexed data from {sourceIndex} to {destinationIndex}");
        }

        public async Task<JObject> GetAliasesAndMeta(string indexName)
        {
            string getMappingUrl = $"{elasticsearchCredential.Url}/{indexName}?filter_path=*.aliases,*.mappings._meta";
            HttpResponseMessage getMappingResponse = await client.GetAsync(getMappingUrl);
            getMappingResponse.EnsureSuccessStatusCode();
            string mappingJson = await getMappingResponse.Content.ReadAsStringAsync();
            Logger.LogMessage(LogLevel.INFO, $"Aliases and Mappings: {JsonConvert.SerializeObject(mappingJson, Formatting.None)}");
            return JObject.Parse(mappingJson);
        }

        public async Task DeleteIndex(string indexName)
        {
            Logger.LogMessage(LogLevel.WARN, $"Deleting index: {indexName}");
            string deleteIndexUrl = $"{elasticsearchCredential.Url}/{indexName}";
            HttpResponseMessage deleteBackupIndexResponse = await client.DeleteAsync(deleteIndexUrl);
            deleteBackupIndexResponse.EnsureSuccessStatusCode();
            Logger.LogMessage(LogLevel.WARN, $"Deleted index: {indexName}");
        }

        public async Task<string> GetCurrentIndexTemplateVersion(string indexName)
        {
            Logger.LogMessage(LogLevel.WARN, $"Fetching current index template for: {indexName}");
            string fetchIndexUrl = $"{elasticsearchCredential.Url}/{indexName}/_mappings?filter_path=**.esMetadata.esTemplateVersion";
            HttpResponseMessage templateVersionResponse = await client.GetAsync(fetchIndexUrl);
            templateVersionResponse.EnsureSuccessStatusCode();
            var versionJson = JObject.Parse(await templateVersionResponse.Content.ReadAsStringAsync());
            var version = versionJson?[indexName]?["mappings"]?["_meta"]?["esMetadata"]?["esTemplateVersion"]?.ToString() ?? "0";
            Logger.LogMessage(LogLevel.INFO, $"Index template version: {JsonConvert.SerializeObject(versionJson, Formatting.None)}");
            return version;
        }

        public async Task<long> GetDocumentCount(string indexName)
        {
            Logger.LogMessage(LogLevel.WARN, $"Fetching count for: {indexName}");
            string countUrl = $"{elasticsearchCredential.Url}/{indexName}/_count";
            HttpResponseMessage countResponse = await client.GetAsync(countUrl);
            countResponse.EnsureSuccessStatusCode();
            var countJson = JObject.Parse(await countResponse.Content.ReadAsStringAsync());
            var count = Convert.ToInt64(countJson["count"].ToString());
            Logger.LogMessage(LogLevel.INFO, $"Count response: {JsonConvert.SerializeObject(countJson, Formatting.None)}");
            return count;
        }

        public async Task TakeSnapShot(string indexName, string ticketId)
        {
            long tick = DateTime.Now.Ticks;
            string backupString = $"backup_{tick}_{indexName}-gbsrch-{ticketId}";
            string snapshotUrl = $"{elasticsearchCredential.Url}/_snapshot/{elasticsearchCredential.Repository}/{backupString}?wait_for_completion=true";
            string requestBody = $"{{ \"indices\": \"{indexName}\", \"ignore_unavailable\": true, \"include_global_state\": false }}";

            Logger.LogMessage(LogLevel.INFO, $"taking backup for index: {indexName}. TicketId {ticketId}. snapshotUrl: {snapshotUrl}");

            var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");
            var response = await client.PutAsync(snapshotUrl, content);
            var s = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var snapshotStatusUrl = $"{elasticsearchCredential.Url}/_snapshot/{elasticsearchCredential.Repository}/{backupString}/_status?filter_path=**.snapshot,**.state,**.{indexName}";
            bool isStatusComplete = false;


            while (!isStatusComplete)
            {
                Logger.PrintDivider();
                var statusResponse = await client.GetAsync(snapshotStatusUrl);
                statusResponse.EnsureSuccessStatusCode();

                var statusResponsejson = JObject.Parse(await statusResponse.Content.ReadAsStringAsync());

                if(statusResponsejson["snapshots"][0]["indices"] == null)
                {
                    throw new Exception("Index does not exist hence aborting the operation");
                }
                if (statusResponsejson["snapshots"][0]["snapshot"].ToString().Equals(backupString) && statusResponsejson["snapshots"][0]["state"].ToString().Equals("SUCCESS") && statusResponsejson["snapshots"][0]["indices"].HasValues)
                {
                    isStatusComplete = true;
                }
                if (statusResponsejson["snapshots"][0]["state"].ToString().Equals("FALIED") || !statusResponsejson["snapshots"][0]["snapshot"].ToString().Equals(backupString) && statusResponsejson["snapshots"][0]["indices"].HasValues)
                {
                    throw new Exception($"Exception occured while taking a backup, snapshotUrl: {snapshotStatusUrl} ");
                }

                Logger.LogMessage(LogLevel.INFO, $"Current status: {statusResponsejson["snapshots"][0]["state"].ToString()}");
                Logger.LogMessage(LogLevel.INFO, $"response object: {JsonConvert.SerializeObject(statusResponsejson, Formatting.None)}");
                Thread.Sleep(1000);
            }
            Logger.PrintDivider();
            Logger.LogMessage(LogLevel.INFO, $"snapshotUrl: {snapshotUrl}");
            Logger.LogMessage(LogLevel.INFO, $"Backups is taken: {indexName}. TicketId {ticketId}");
        }

        public async Task ExecutePainlessScriptWhileReindexing(string requestBody)
        {
            var reindexUrl = $"{elasticsearchCredential.Url}/_reindex?wait_for_completion=false";

            var response = await client.PostAsync(reindexUrl, new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

        }

    }
}
