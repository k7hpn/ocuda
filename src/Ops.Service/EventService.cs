using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;

namespace Ocuda.Ops.Service
{
    public class EventService : BaseService<EventService> //, IEventService
    {
        public EventService(ILogger<EventService> logger,
            IHttpContextAccessor httpContextAccessor) : base(logger, httpContextAccessor)
        {
        }
    }
}
