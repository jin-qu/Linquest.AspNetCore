using Microsoft.AspNetCore.Mvc;

// todo: Query handling

namespace Linquest.AspNetCore {

    public class LinquestController : ControllerBase, ILinquestService { }

    public interface ILinquestService { }
}
