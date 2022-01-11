using System;
using System.Threading.Tasks;
namespace FfAdmin.Test
{
    public interface ITemporaryDatabase : IAsyncDisposable, IDisposable
    {
        Task UseTemporaryDatabase(string name);
    }
}
