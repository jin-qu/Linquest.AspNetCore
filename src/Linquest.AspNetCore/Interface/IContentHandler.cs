namespace Linquest.AspNetCore.Interface;

public interface IContentHandler<in T>: IContentHandler {
    ProcessResult HandleContent(T value, ActionContext context);
}

public interface IContentHandler {
    ProcessResult HandleContent(object value, ActionContext context);
}
