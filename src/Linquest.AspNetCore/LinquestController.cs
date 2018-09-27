using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore {
    using Interface;

    public class LinquestController : ControllerBase, ILinquestService {

        public event BeforeQueryDelegate BeforeHandleQuery;
        public event BeforeQueryDelegate BeforeQueryExecute;
        public event AfterQueryDelegate AfterQueryExecute;
        public int? MaxResultCount { get; set; }

        public ProcessResult ProcessRequest(ActionContext actionContext) {
            return Helper.DefaultRequestProcessor(actionContext, this.HttpContext.RequestServices);
        }

        void ILinquestService.OnBeforeHandleQuery(BeforeQueryEventArgs args) {
            BeforeHandleQuery?.Invoke(this, args);
        }

        void ILinquestService.OnBeforeQueryExecute(BeforeQueryEventArgs args) {
            BeforeQueryExecute?.Invoke(this, args);
        }

        void ILinquestService.OnAfterQueryExecute(AfterQueryEventArgs args) {
            AfterQueryExecute?.Invoke(this, args);
        }
    }
}
