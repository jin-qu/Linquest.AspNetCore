using Microsoft.AspNetCore.Mvc;

namespace Linquest.AspNetCore {

    public class LinquestController : ControllerBase, ILinquestService { }

    public interface ILinquestService { }
}
