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
using System.Linq;
using System.Threading.Tasks;

namespace TokenManagerFunctions
{
    public static class ServeTokenFunction
    {
        [FunctionName("ServeTokenFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] IDocumentClient client,
            ILogger log)
        {
            log.LogInformation("ServeTokenFunction - Invoked");

            TransactionTypeEnume transactionType;
            Token token = new Token();
            Enum.TryParse(request.Query["transactionType"], out transactionType);
            int counterNo = int.Parse(request.Query["CounterNo"]);

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(token=>token.TransactionType == transactionType && token.Status == TokenStatusEnume.InQueue)
                .OrderBy(token => token.TokenNo)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                token = (Token)queryRes.ExecuteNextAsync().Result.First();
                token.Status = TokenStatusEnume.InCounter;
                token.CurrentEstimatedWaitingTime = 0;
                token.CounterNo = counterNo;
                await client.CreateDocumentAsync(tokenCollectUri, token);
            }

            log.LogInformation("ServeTokenFunction - Completed");
            return new OkObjectResult(token);
        }
    }
}
