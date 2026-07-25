namespace ImageApi.Models;

// One row for the review table (image fields are null on a "no_match" pairing).
public sealed record PairingRow(
    int Id, string PostSlug, string? Filename, string? Subject,
    double? Similarity, string Status, string? Reason);