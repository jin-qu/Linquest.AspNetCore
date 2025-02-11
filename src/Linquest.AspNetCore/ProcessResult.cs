namespace Linquest.AspNetCore;

public class ProcessResult(ActionContext context) {
    public ActionContext Context { get; } = context;
    public object? Result { get; set; }
    public int? InlineCount { get; set; }
}
