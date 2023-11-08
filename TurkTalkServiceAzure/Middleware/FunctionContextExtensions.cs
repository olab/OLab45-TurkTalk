using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace OLab.TurkTalk.Service.Azure.Middleware;

/// <summary>
/// Helper Function for FunctionContext.
/// This will available to functioncontext as a extension methods.
/// </summary>
public static class FunctionContextExtentions
{
    /// <summary>
    /// Gets the headers for a function context
    /// </summary>
    /// <param name="functionContext"></param>
    /// <returns>headers as a dictionary</returns>
    public static Dictionary<string, string> GetHttpRequestHeaders(this FunctionContext functionContext)
    {
        var headers = new Dictionary<string, string>();
        if (!functionContext.BindingContext.BindingData.TryGetValue("Headers", out var headersObj))
            return headers;

        if (headersObj is not string)
            return headers;

        var headersStr = headersObj as string;

        // Deserialize headers from JSON
        headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headersStr);
        var normalizedKeyHeaders = headers.ToDictionary(h => h.Key.ToLowerInvariant(), h => h.Value);

        return normalizedKeyHeaders;
    }

    /// <summary>
    /// Check that current inputbinding or current endpoint function in HttpTrigger or not.
    /// </summary>
    /// <param name="functionContext"></param>
    /// <returns>return true or false.</returns>
    public static bool IsHttpTriggerFunction(this FunctionContext functionContext)
    {
        if (functionContext.FunctionDefinition.InputBindings != null &&
            functionContext.FunctionDefinition.InputBindings.ContainsKey("req"))
        {
            var bindingMetaData = functionContext.FunctionDefinition.InputBindings["req"];
            return bindingMetaData.Direction == BindingDirection.In && "httptrigger".Equals(bindingMetaData.Type, StringComparison.InvariantCultureIgnoreCase);

        }
        return false;
    }

    /// <summary>
    /// This method extract requestdata from functioncontext.
    /// https://github.com/Azure/azure-functions-dotnet-worker/issues/414
    /// </summary>
    /// <param name="functionContext"></param>
    /// <returns>return HttpRequestData.</returns>
    public static HttpRequestData GetHttpRequestData(this FunctionContext functionContext)
    {
        try
        {
            var keyValuePair = functionContext.Features.SingleOrDefault(f => f.Key.Name == "IFunctionBindingsFeature");
            var functionBindingsFeature = keyValuePair.Value;
            var type = functionBindingsFeature.GetType();
            var inputData = type.GetProperties().Single(p => p.Name == "InputData").GetValue(functionBindingsFeature) as IReadOnlyDictionary<string, object>;
            return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set response data for request.
    /// https://github.com/Azure/azure-functions-dotnet-worker/issues/414
    /// </summary>
    /// <param name="functionContext"></param>
    /// <param name="responseData"></param>
    public static void SetResponseData(this FunctionContext functionContext, HttpResponseData responseData)
    {
        var feature = functionContext.Features.FirstOrDefault(f => f.Key.Name == "IFunctionBindingsFeature").Value;
        if (feature == null) throw new Exception("Required binding feature is not present.");
        var pinfo = feature.GetType().GetProperty("InvocationResult");
        pinfo.SetValue(feature, responseData);
    }

    /// <summary>
    /// Create response from function context and with specified object and status code.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="functionContext"></param>
    /// <param name="statusCode"></param>
    /// <param name="data"></param>
    /// <returns>return task.</returns>
    public static async Task CreateJsonResponse<T>(this FunctionContext functionContext, System.Net.HttpStatusCode statusCode, T data)
    {
        var request = functionContext.GetHttpRequestData();
        if (request != null)
        {
            var response = request.CreateResponse(statusCode);
            await response.WriteAsJsonAsync(data);
            response.StatusCode = statusCode;
            functionContext.SetResponseData(response);
        }
    }
}