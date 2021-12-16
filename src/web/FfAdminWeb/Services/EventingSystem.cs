using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FfAdmin.AdminModule;
using FfAdmin.Common;
using FfAdmin.EventStore;

namespace FfAdminWeb.Services
{
    public interface IEventingSystem
    {
        bool HasSession { get; }
        Task ImportEvent(Event e, bool process = false);
        Task ImportEvents(IEnumerable<Event> es, bool process = false);
        Task<CoreMessage> ProcessEvents(DateTime until);
    }
    public class EventingSystem : IEventingSystem
    {
        private readonly IEventStore _eventStore;
        private readonly IEventRepository _eventRepository;

        public EventingSystem(IEventStore eventStore, IEventRepository eventRepository)
        {
            _eventStore = eventStore;
            _eventRepository = eventRepository;
        }

        public bool HasSession => _eventStore.HasSession;

        public async Task ImportEvent(Event e, bool process)
        {
            _eventStore.WriteEvent(e);
            await _eventRepository.Import(_eventStore.FileTimestamp!.Value, new[] { e });
            if (process)
                await ProcessEvents(DateTime.UtcNow);
        }
        public async Task ImportEvents(IEnumerable<Event> es, bool process)
        {
            var events = es.ToArray();
            foreach (var e in events)
                _eventStore.WriteEvent(e);
            await _eventRepository.Import(_eventStore.FileTimestamp!.Value, events);
            if (process)
                await ProcessEvents(DateTime.UtcNow);
        }

        public Task<CoreMessage> ProcessEvents(DateTime until)
            => _eventRepository.ProcessEvents(until);
    }
}