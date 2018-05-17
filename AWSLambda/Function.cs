using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using CCWallet.AWSLambda.Entities;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace CCWallet.AWSLambda
{
    public class Function
    {
        public static async Task<APIGatewayProxyResponse> FunctionHandlerAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            switch (request.PathParameters["model"])
            {
                case "guild":
                    return await OptionResourceAsync<GuildPreference>(request);

                case "group":
                    return await OptionResourceAsync<GroupPreference>(request);

                case "user":
                    return await OptionResourceAsync<UserPreference>(request);

                case "log":
                    return await OptionResourceAsync<CommandLog>(request);

                default:
                    throw new NotSupportedException();
            }
        }

        private static async Task<APIGatewayProxyResponse> OptionResourceAsync<T>(APIGatewayProxyRequest request) where T : class, IDynamoTable
        {
            var id = UInt64.Parse(request.PathParameters["id"]);
            var since = request.QueryStringParameters?.ContainsKey("since") ?? false ? UInt64.Parse(request.QueryStringParameters["since"]) : 0;
            var option = default(T);

            using (var client = new AmazonDynamoDBClient())
            using (var dynamo = new DynamoDBContext(client))
            {
                switch (request.HttpMethod)
                {
                    case "GET":
                        option = await dynamo.LoadAsync<T>(id);
                        return option == null ? Response(404) : (option.GetHashKey() <= since ? Response(204) : Response(option));

                    case "PUT":
                    case "POST":
                        option = JsonConvert.DeserializeObject<T>(request.Body);
                        await dynamo.SaveAsync(option);
                        return Response();

                    case "DELETE":
                        await dynamo.DeleteAsync<T>(id);
                        return Response();

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private static APIGatewayProxyResponse Response(int statusCode = 200)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = statusCode,
            };
        }

        private static APIGatewayProxyResponse Response<T>(T obj, int statusCode = 200) where T : class
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = statusCode,
                Body = JsonConvert.SerializeObject(obj),
            };
        }
    }
}
