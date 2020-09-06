using Microsoft.VisualStudio.TestTools.UnitTesting;
using TokenManagerFunctions;

namespace TokenManager.Test
{
    [TestClass]
    public class TokenManagerFunctionTest : MockDataTest
    {
        [TestMethod]
        public void EstimatedWaitTimeFunctionTest()
        {
            var request = GenerateHttpRequestMock(string.Empty);
            var docDbClient = GenerateCosmosDBClientMock();
            var response = EstimateWaitingTimeFunction.Run(request.Object, docDbClient.Object, logger);
        }
    }
}
