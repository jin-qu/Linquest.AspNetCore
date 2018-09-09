namespace Linquest.AspNetCore {

    public class ProcessResult {

        public ProcessResult (ActionContext actionContext) {
            ActionContext = actionContext;
        }

        public ActionContext ActionContext { get; }

        public object Result { get; set; }

        public int? InlineCount { get; set; }
    }
}
