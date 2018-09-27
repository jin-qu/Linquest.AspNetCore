namespace Linquest.AspNetCore.Interface {

    public interface ILinquestService {
        
        event BeforeQueryDelegate BeforeHandleQuery;

        void OnBeforeHandleQuery(BeforeQueryEventArgs args);

        event BeforeQueryDelegate BeforeQueryExecute;

        void OnBeforeQueryExecute(BeforeQueryEventArgs args);

        event AfterQueryDelegate AfterQueryExecute;

        void OnAfterQueryExecute(AfterQueryEventArgs args);

        int? MaxResultCount { get; set; }

        ProcessResult ProcessRequest(ActionContext context);
    }
}