public interface IOpenAIEndpointsHandler
{
    Task<IResult> ChatCompletion(ChatCompletionRequest request);
}