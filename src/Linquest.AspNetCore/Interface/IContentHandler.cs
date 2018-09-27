namespace Linquest.AspNetCore.Interface {

    public interface IContentHandler<in T> {

        ProcessResult HandleContent(T value, ActionContext actionContext);
    }
}
