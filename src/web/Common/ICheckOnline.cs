using System.Threading.Tasks;

namespace FfAdmin.Common;

public interface ICheckOnline
{
    Task<bool> IsOnline();
}
