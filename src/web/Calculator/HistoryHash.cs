using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FfAdmin.Calculator;

public record HistoryHash : IModel<HistoryHash>
{
    public HistoryHash() : this(new byte[32])
    {
        
    }
    public HistoryHash(byte[] hash)
    {
        if(hash is null)
            throw new ArgumentNullException(nameof(hash));
        if(hash.Length!=32)
            throw new ArgumentException("Hash must be 32 bytes long", nameof(hash));
        Hash = hash;
    }
    public byte[] Hash { get; }
    public static HistoryHash Empty { get; } = new();
    public static IEventProcessor<HistoryHash> Processor { get; } = new Impl();

    private class Impl : EventProcessor<HistoryHash>
    {
        private static readonly SHA256 Sha256 = SHA256.Create();
        public override HistoryHash Start => Empty;
        protected override HistoryHash Default(HistoryHash model, IContext context, Event e)
        {
            using var ms = new MemoryStream();
            ms.Write(model.Hash);
            ms.Write(Encoding.UTF8.GetBytes(e.ToJsonString()));
            
            var hash = Sha256.ComputeHash(ms.ToArray());
            return new(hash);
        }
    }
}
