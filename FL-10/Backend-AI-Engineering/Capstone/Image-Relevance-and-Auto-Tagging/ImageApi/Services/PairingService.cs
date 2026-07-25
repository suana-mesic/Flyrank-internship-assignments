using ImageApi.Repositories;

namespace ImageApi.Services;

public sealed class PairingService
{
    private readonly PostRepository _posts;
    private readonly MatchRepository _match;
    private readonly GuardService _guard;
    private readonly PairingRepository _pairings;

    public PairingService(PostRepository posts, MatchRepository match,
        GuardService guard, PairingRepository pairings)
    {
        _posts = posts; _match = match; _guard = guard; _pairings = pairings;
    }

    // Computes the best image for a post, runs the guard, and stores the result
    // as a pairing (either "suggested" with an image, or "no_match").
    public object? SuggestForPost(int postId)
    {
        var post = _posts.GetById(postId);
        if (post is null) return null;

        var ranked = _match.RankImagesForPost(postId, 1);
        if (ranked.Count == 0)
        {
            var emptyId = _pairings.Suggest(postId, null, null, "no_match", "No embedded images to match.");
            return new { pairingId = emptyId, status = "no_match", reason = "No embedded images." };
        }

        var best = ranked[0];
        var verdict = _guard.Evaluate(post.Value.topic, best.subject, best.similarity);

        if (verdict.Decision == GuardDecision.Accept)
        {
            var pid = _pairings.Suggest(postId, best.imageId, best.similarity, "suggested", verdict.Reason);
            return new
            {
                pairingId = pid,
                status = "suggested",
                image = best.filename,
                best.subject,
                similarity = Math.Round(best.similarity, 4),
                reason = verdict.Reason
            };
        }

        var noId = _pairings.Suggest(postId, null, best.similarity, "no_match", verdict.Reason);
        return new
        {
            pairingId = noId,
            status = "no_match",
            reason = verdict.Reason,
            bestCandidate = best.filename
        };
    }
}