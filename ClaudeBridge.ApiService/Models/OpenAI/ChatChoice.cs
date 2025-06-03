public class ChatChoice
{
    public int Index { get; set; }
    public ChatMessage Message { get; set; } = new();
    public ChatMessage? Delta { get; set; }
    public string? FinishReason { get; set; }
}