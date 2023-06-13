using System.Threading.Tasks;

namespace FfAdmin.Calculator.Core;

public interface IEventSequence
{
    ValueTask<int> Count { get; }
    Task<Event> this[int index] { get; }
    Task<IEventSequence> Append(IEnumerable<Event> events);
}
