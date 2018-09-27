using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore {
    using Interface;

    public class LinquestController : ControllerBase, ILinquestService {

        public event BeforeQueryDelegate BeforeHandleQuery;
        public event BeforeQueryDelegate BeforeQueryExecute;
        public event AfterQueryDelegate AfterQueryExecute;
        public int? MaxResultCount { get; set; }

        protected virtual ProcessResult ProcessRequest(ActionContext context) {
            return Helper.DefaultRequestProcessor(context, this.HttpContext.RequestServices);
        }

        ProcessResult ILinquestService.ProcessRequest(ActionContext context) {
            return ProcessRequest(context);
        }

        protected void OnBeforeHandleQuery(BeforeQueryEventArgs args) {
            BeforeHandleQuery?.Invoke(this, args);
        }

        void ILinquestService.OnBeforeHandleQuery(BeforeQueryEventArgs args) {
            OnBeforeHandleQuery(args);
        }

        protected void OnBeforeQueryExecute(BeforeQueryEventArgs args) {
            BeforeQueryExecute?.Invoke(this, args);
        }

        void ILinquestService.OnBeforeQueryExecute(BeforeQueryEventArgs args) {
            OnBeforeQueryExecute(args);
        }

        protected void OnAfterQueryExecute(AfterQueryEventArgs args) {
            AfterQueryExecute?.Invoke(this, args);
        }

        void ILinquestService.OnAfterQueryExecute(AfterQueryEventArgs args) {
            OnAfterQueryExecute(args);
        }
    }
}
