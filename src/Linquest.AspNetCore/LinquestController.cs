using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore {
    using Interface;

    [LinquestActionFilter]
    public class LinquestController : ControllerBase, ILinquestService {

        public event BeforeQueryDelegate BeforeHandleQuery;
        public event BeforeQueryDelegate BeforeQueryExecute;
        public event AfterQueryDelegate AfterQueryExecute;
        public int? MaxResultCount { get; set; }

        protected virtual ProcessResult ProcessRequest(ActionContext context) =>
            Helper.DefaultRequestProcessor(context, this.HttpContext.RequestServices);

        ProcessResult ILinquestService.ProcessRequest(ActionContext context) =>
            ProcessRequest(context);

        void ILinquestService.OnBeforeHandleQuery(BeforeQueryEventArgs args) =>
            BeforeHandleQuery?.Invoke(this, args);

        void ILinquestService.OnBeforeQueryExecute(BeforeQueryEventArgs args) =>
            BeforeQueryExecute?.Invoke(this, args);

        void ILinquestService.OnAfterQueryExecute(AfterQueryEventArgs args) =>
            AfterQueryExecute?.Invoke(this, args);
    }
}
