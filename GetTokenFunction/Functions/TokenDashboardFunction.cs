using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Models;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;
using System.Globalization;
using System.Linq;
using Helpers;

namespace TokenManagerFunctions.Functions
{
    public static class TokenDashboardFunction
    {
        [FunctionName("TokenDashboardFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("TokenDashboardFunction - Completed");

            DateTime date;
            if (!DateTime.TryParseExact(request.Query["date"], "dd-MM-yyyy", null, DateTimeStyles.None, out date))
            {
                return new BadRequestObjectResult("Input error : Please provide query string 'date' in 'dd-MM-yyyy' format.");
            }

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            List<TokenDashboardData> dashboardData = new List<TokenDashboardData>();

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .OrderBy(token => token.TokenNo)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                dashboardData = queryRes.ExecuteNextAsync().Result
                    .Where(token => token.CreatedDate.Date == date.Date)
                        .Select(x => new TokenDashboardData()
                        {
                            TokenNo = x.TokenNo,
                            Service = TermHelper.GetEnumeValue(x.TransactionType.ToString(), typeof(TransactionTypeEnume)),
                            Status = TermHelper.GetEnumeValue(x.Status.ToString(), typeof(TokenStatusEnume)),
                            Action = TermHelper.GetAction(x.Status.ToString())
                        }).ToList();
            }

            log.LogInformation("TokenDashboardFunction - Completed");

            return new OkObjectResult(dashboardData.OrderBy(x => x.TokenNo));
        }
    }
}
