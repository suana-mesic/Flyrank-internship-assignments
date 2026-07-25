namespace ImageApi.Models;

// Vision call result: the tags plus token counts reported by the model.
public sealed record VisionResult(ImageTags Tags, int PromptTokens, int OutputTokens);