using System.ComponentModel;

public class ChatMessage
{
    [DefaultValue("user")]
    public string Role { get; set; } = "user";

    [DefaultValue("Give me a Hello World")]
    public string Content { get; set; } = "Give me a Hello World";
}