namespace Linquest.AspNetCore {

    public class ProcessResult {

        public ProcessResult (ActionContext context) {
            Context = context;
        }

        public ActionContext Context { get; }

        public object Result { get; set; }

        public int? InlineCount { get; set; }
    }
}
