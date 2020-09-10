using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TokenManager.Test
{
    public interface IFakeDocumentQuery<T> : IDocumentQuery<T>, IOrderedQueryable<T>
    {
    }
    public class MockDataTest
    {
        public readonly ILogger logger = NullLoggerFactory.Instance.CreateLogger("Test");
        public DefaultHttpRequest GenerateHttpRequest(string param)
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext());
            Dictionary<string, StringValues> queryParams = null;
            if (!string.IsNullOrEmpty(param))
            {
                queryParams = new Dictionary<string, StringValues>() { { "date", param } };
            }
            request.Query = new QueryCollection(queryParams);
            return request;
        }
        public Mock<IDocumentClient> GenerateCosmosDBClientMock()
        {
            var response = new FeedResponse<Token>(new List<Token>() { new Token() { 
            TokenNo = "1",
            CounterNo = 1,
            TransactionType = TransactionTypeEnume.BankTransaction,
            Status = TokenStatusEnume.InQueue
            }});

            var mockDocumentQuery = new Mock<IFakeDocumentQuery<Token>>();
            mockDocumentQuery
                .SetupSequence(_ => _.HasMoreResults)
                .Returns(true)
                .Returns(false);
            mockDocumentQuery
                .Setup(_ => _.ExecuteNextAsync<Token>(It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var client  =  new Mock<IDocumentClient>();
            client.Setup(_ => _.CreateDocumentQuery<Token>(It.IsAny<Uri>(), It.IsAny<FeedOptions>()))
            .Returns(mockDocumentQuery.Object);

            return client;
        }

        public Mock<HttpRequest> GenerateHttpRequestMock(string param)
        {
            var req = new Mock<HttpRequest>();
            Dictionary<string, StringValues> queryParams = null;
            if (!string.IsNullOrEmpty(param))
            {
                queryParams = new Dictionary<string, StringValues>() { { "date", param } };
            }
            req.Setup(r => r.Query).Returns(new QueryCollection(queryParams));
            return req;
        }
    }
}
