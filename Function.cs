using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;
public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"FunctionHandler received: {input}");
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            
            string issueUrl = json?.issue?.html_url;
            if (string.IsNullOrEmpty(issueUrl))
            {
                context.Logger.LogInformation("No issue URL found in payload");
                return "No issue URL found";
            }
            
            string payload = $"{{\"text\":\"Issue Created: {issueUrl}\"}}";
            
            var client = new HttpClient();
            var webhookUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            var webRequest = new HttpRequestMessage(HttpMethod.Post, webhookUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        
            var response = client.Send(webRequest);
            using var reader = new StreamReader(response.Content.ReadAsStream());
                
            return reader.ReadToEnd();
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}