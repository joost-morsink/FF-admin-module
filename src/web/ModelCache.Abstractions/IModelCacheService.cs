using System.Threading.Tasks;
using FfAdmin.Common;

namespace FfAdmin.ModelCache.Abstractions;

public interface IModelCacheService
{
    Task<string[]> GetBranches();
    Task ClearCache();
    Task RemoveBranch(string branchName);
    Task RemoveModel(string type);
    Task<HashesForBranch> GetHashesForBranch(string branchName);
    Task PutHashesForBranch(string branchName, HashesForBranch data);
    Task<string[]> GetTypesForHash(HashValue hash);
    Task<byte[]?> GetData(HashValue hash, string type);
    Task PutData(HashValue hash, string type, byte[] data);
    Task<bool> RunGarbageCollection();
}
