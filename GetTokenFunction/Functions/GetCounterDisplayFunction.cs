using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using Models;
using System.Collections.Generic;

namespace TokenManagerFunctions.Functions
{
    public static class GetCounterDisplayFunction
    {
        [FunctionName("GetCounterDisplayFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("GetCounterDisplayFunction - Invoked");

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            List<CounterDisplay> counterDisplays = new List<CounterDisplay>();

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(token => token.Status == TokenStatusEnume.InCounter)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                foreach (Token token in queryRes.ExecuteNextAsync().Result)
                {
                    var counterData = counterDisplays.Where(x => x.CounterNo == token.CounterNo);
                    if (counterData != null && counterData.Any())
                    {
                        counterData.First().TokenNo = token.TokenNo;
                    }
                    else
                    {
                        counterDisplays.Add(new CounterDisplay() { TokenNo = token.TokenNo, CounterNo = token.CounterNo.Value });
                    }
                }
            }

            log.LogInformation("GetCounterDisplayFunction - Completed");

            return new OkObjectResult(counterDisplays.OrderBy(x=>x.CounterNo));
        }
    }
}
