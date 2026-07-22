namespace ImageApi.Models;

// The structured result the vision model returns for one image.
public sealed record ImageTags(
    string Subject,        // the main thing, e.g. "red fox"
    string Category,       // coarse class, e.g. "animal"
    string[] Attributes,   // short visual descriptors, e.g. ["orange fur","snow"]
    string Caption,        // one-sentence description (this is what we embed later)
    double Confidence      // 0..1, how sure the model is about the subject
);