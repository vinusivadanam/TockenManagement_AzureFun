using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TokenManagerFunctions
{
    public static class GetCounterDisplayFunction
    {
        [FunctionName("GetCounterDisplayFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] IDocumentClient client,
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
