namespace Linquest.AspNetCore.Tests.Fixture;

using Interface;
using Model;

public class ProductHandler : IContentHandler<Product> {

    public ProcessResult HandleContent(object value, ActionContext context)
        => HandleContent((Product)value, context);
            
    public ProcessResult HandleContent(Product value, ActionContext context)
        => new ProcessResult(context) { Result = 42 };
}
