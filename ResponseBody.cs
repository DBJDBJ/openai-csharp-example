namespace openai_csharp_example;

struct Message
{
    public string Content { get; set; }
}

struct Choice
{
    public Message Message { get; set; }
}

struct ResponseBody
{
    public List<Choice> Choices { get; set; }
}
