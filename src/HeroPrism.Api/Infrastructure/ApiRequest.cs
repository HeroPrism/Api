using System;
using Nerdino.Controllerless;

namespace HeroPrism.Api.Infrastructure
{
    public class ApiRequest : Attribute, IAuthenticatedAction
    {
        public string Route { get; }
        public string Group { get; }
        public ActionType ActionType { get; }
        public bool AllowAnonymous { get; }

        public ApiRequest(string @group, string route, ActionType actionType, bool allowAnonymous = false)
        {
#if DEBUG
            allowAnonymous = true;
#endif
            AllowAnonymous = allowAnonymous;
            ActionType = actionType;
            Group = @group;
            Route = route;
        }
    }
}