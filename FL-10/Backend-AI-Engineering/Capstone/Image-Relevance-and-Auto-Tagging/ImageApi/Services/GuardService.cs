using System.Globalization;

namespace ImageApi.Services
{
    public enum GuardDecision { Accept, RejectLowSimilarity, RejectTagMismatch }
    public sealed record GuardResult(GuardDecision Decision, string Reason);
    public sealed class GuardService
    {
        private readonly double _threshold;

        public GuardService(IConfiguration config)
        {
            _threshold = double.TryParse(config["MATCH:SimilarityThreshold"],
                NumberStyles.Float, CultureInfo.InvariantCulture, out var t) ? t : 0.72;
        }

        // Decides whether an image may illustrate a post.
        // Rule A: similarity below threshold -> no confident match.
        // Rule B: the image subject does not match the post topic -> tag mismatch.
        public GuardResult Evaluate(string postTopic, string imageSubject, double similarity)
        {
            if (similarity < _threshold)
                return new(GuardDecision.RejectLowSimilarity,
                    $"No confident match: similarity {similarity:0.00} is below threshold {_threshold:0.00}.");

            if (!TagsAgree(postTopic, imageSubject))
                return new(GuardDecision.RejectTagMismatch,
                    $"Tags disagree: image shows '{imageSubject}', but the post is about '{postTopic}'.");

            return new(GuardDecision.Accept,
                $"Accepted: image '{imageSubject}' matches topic '{postTopic}' (similarity {similarity:0.00}).");
        }

        // Agreement = the image subject contains the post topic word (case-insensitive),
        // e.g. topic "fox" is inside subject "red fox" / "arctic fox", but not "wolf".
        private static bool TagsAgree(string postTopic, string imageSubject)
        {
            var topic = postTopic.Trim().ToLowerInvariant();
            var subject = imageSubject.ToLowerInvariant();
            return topic.Length > 0 && subject.Contains(topic);
        }
    }
}
