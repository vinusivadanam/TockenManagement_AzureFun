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
using Models;
using Microsoft.Azure.Documents.Linq;
using System.Linq;

namespace TokenManagerFunctions
{
    public static class UpdateTokenFunction
    {
        [FunctionName("UpdateTokenFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("UpdateTokenFunction - Invoked");

            TokenStatusEnume status;
            Token token = new Token();
            Enum.TryParse(request.Query["status"], out status);
            string tokenNo = request.Query["tokenNo"];

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(token => token.TokenNo == tokenNo)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                token = (Token)queryRes.ExecuteNextAsync().Result.First();
                token.Status = status;
                token.CurrentEstimatedWaitingTime = 0;
                await client.CreateDocumentAsync(tokenCollectUri, token);
            }

            log.LogInformation("UpdateTokenFunction - Completed");
            return new OkObjectResult(token);
        }
    }
}
