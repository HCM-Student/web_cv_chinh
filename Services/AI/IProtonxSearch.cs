using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WEB_CV.Services.AI
{
    public record ProtonxHit(string Id, double Score, string? Title, string? Url, string? Snippet);
    public interface IProtonxSearch
    {
        Task<IReadOnlyList<ProtonxHit>> SearchAsync(string query, int n = 5, CancellationToken ct = default);
    }
}

