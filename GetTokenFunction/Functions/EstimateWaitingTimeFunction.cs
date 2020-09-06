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
using System.Collections.Generic;
using Models;
using System.Linq;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.Documents;

namespace TokenManagerFunctions
{
    public static class EstimateWaitingTimeFunction
    {
        [FunctionName("EstimateWaitingTimeFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] IDocumentClient client,
            ILogger log)
        {
            log.LogInformation("EstimateWaitingTimeFunction - Invoked");

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            List<WaitingTimeDisplay> tokenWaitList = new List<WaitingTimeDisplay>();

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(token => token.Status == TokenStatusEnume.InQueue)
                .OrderBy(token => token.TokenNo)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                foreach (Token token in queryRes.ExecuteNextAsync().Result)
                {
                    int estWaitTime = 0;
                    if (token.TransactionType == TransactionTypeEnume.BankTransaction)
                    {
                        estWaitTime = (tokenWaitList.Where(x => x.TransactionType == token.TransactionType).Count() + 1) * 5;
                    }
                    else
                    {
                        estWaitTime = (tokenWaitList.Where(x => x.TransactionType == token.TransactionType).Count() + 1) * 25;
                    }

                    tokenWaitList.Add(new WaitingTimeDisplay()
                    {
                        TokenNo = token.TokenNo,
                        EstimatedWaitingTime = estWaitTime,
                        TransactionType = token.TransactionType
                    });
                }
            }

            log.LogInformation("EstimateWaitingTimeFunction - Completed");

            return new OkObjectResult(tokenWaitList.OrderBy(x => x.TokenNo));
        }
    }
}
