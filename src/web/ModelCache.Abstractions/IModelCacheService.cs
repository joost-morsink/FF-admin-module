using System.Threading.Tasks;

namespace FfAdmin.ModelCache.Abstractions;

public interface IModelCacheService
{
    Task RemoveBranch(string branchName);
    Task<HashesForBranch> GetHashesForBranch(string branchName);
    Task PutHashesForBranch(string branchName, HashesForBranch data);
    Task<byte[]?> GetData(byte[] hash, string type);
    Task PutData(byte[] hash, string type, byte[] data);
}
