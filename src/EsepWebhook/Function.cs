using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using System.Text;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace EsepWebhook;

public class Function
{
    private static readonly HttpClient client = new HttpClient();

    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Received body: {request.Body}");
            dynamic json = JsonConvert.DeserializeObject(request.Body);
            string issueUrl = json?.issue?.html_url;
            
            var slackMessage = JsonConvert.SerializeObject(new { text = $"Issue Created: {issueUrl}" });
            var slackRequest = new HttpRequestMessage(HttpMethod.Post, Environment.GetEnvironmentVariable("SLACK_URL"))
            {
                Content = new StringContent(slackMessage, Encoding.UTF8, "application/json")
            };

            var response = client.Send(slackRequest);

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Posted to Slack successfully"
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = $"Exception: {ex.Message}"
            };
        }
    }
}
