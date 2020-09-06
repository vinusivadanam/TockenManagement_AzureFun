using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using Models;
using Microsoft.Azure.Documents.Client;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Linq;
using System.Linq;

namespace TokenManagerFunctions.Functions
{
    public static class GetDailyReportFunction
    {
        [FunctionName("GetDailyReportFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest request,
            [CosmosDB(ConnectionStringSetting = "DBConnectionString")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("GetDailyReportFunction - Completed");

            DateTime date;
            if (!DateTime.TryParseExact(request.Query["date"], "dd-MM-yyyy", null, DateTimeStyles.None, out date))
            {
                return new BadRequestObjectResult("Input error : Please provide query string 'date' in 'dd-MM-yyyy' format.");
            }

            Uri tokenCollectUri = UriFactory.CreateDocumentCollectionUri("TokenManagerDB", "Token");
            var options = new FeedOptions { EnableCrossPartitionQuery = true };
            List<DailyCounterServiceReport> serviceReportList = new List<DailyCounterServiceReport>();

            IDocumentQuery<Token> queryRes = client.CreateDocumentQuery<Token>(tokenCollectUri, options)
                .Where(token => token.Status != TokenStatusEnume.InQueue)
                .OrderBy(token => token.TokenNo)
                .AsDocumentQuery();

            if (queryRes.HasMoreResults)
            {
                foreach (Token token in queryRes.ExecuteNextAsync().Result)
                {
                    var counterData = serviceReportList.Where(x => x.CounterNo == token.CounterNo && token.CreatedDate.Date == date.Date);
                    if (counterData != null && counterData.Any())
                    {
                        counterData.First().ServiceCount++;
                    }
                    else
                    {
                        serviceReportList.Add(new DailyCounterServiceReport() { CounterNo = token.CounterNo.Value, ServiceCount = 1, ServiceType = token.TransactionType.ToString() });
                    }
                }
            }

            log.LogInformation("GetDailyReportFunction - Completed");

            return new OkObjectResult(serviceReportList.OrderBy(x => x.CounterNo));
        }
    }
}
