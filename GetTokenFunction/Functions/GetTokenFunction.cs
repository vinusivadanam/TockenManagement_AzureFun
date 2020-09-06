using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TokenManagerFunctions
{
    public static class GetTokenFunction
    {
        [FunctionName("GetTokenFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] IDocumentClient client,
            ILogger log
            )
        {
            log.LogInformation("GetTokenFunction - Invoked");

            Token newToken = JsonConvert.DeserializeObject<Token>(await request.Content.ReadAsStringAsync());

            newToken.CreatedDate = DateTime.Now;
            newToken.Status = TokenStatusEnume.InQueue;

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .OrderByDescending(token => token.TokenNo)
                .AsDocumentQuery();

            options.MaxItemCount = null;

            IDocumentQuery<Token> querActiveToken = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(x => x.Status != TokenStatusEnume.Served && x.TransactionType == newToken.TransactionType)
                .AsDocumentQuery();

            int count = 0;
            if (querActiveToken.HasMoreResults)
            {
                foreach (Token token in querActiveToken.ExecuteNextAsync<Token>().Result)
                {
                    count++;
                }
            }

            if (newToken.TransactionType == TransactionTypeEnume.BankTransaction)
            {
                newToken.InitialEstimatedWaitingTime = 5 * count;
            }
            else
            {
                newToken.InitialEstimatedWaitingTime = 25 * count;
            }
            newToken.CurrentEstimatedWaitingTime = newToken.InitialEstimatedWaitingTime;

            if (queryRes.HasMoreResults)
            {
                var lastToken = (Token)queryRes.ExecuteNextAsync().Result.First();
                newToken.TokenNo = (int.Parse(lastToken.TokenNo) + 1).ToString();
                await client.CreateDocumentAsync(tokenCollectUri, newToken);
            }

            log.LogInformation("GetTokenFunction - Completed");
            return new OkObjectResult(newToken);
        }
    }
}
