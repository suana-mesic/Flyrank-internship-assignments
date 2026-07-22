using ImageApi.Repositories;

namespace ImageApi.Services
{
    public sealed class IngestService
    {
        private readonly ImageRepository _images;
        private readonly PostRepository _posts;

        public IngestService(ImageRepository images, PostRepository posts)
        {
            _images = images;
            _posts = posts;
        }

        // Scans the corpus folder and inserts every .jpg as an image row.
        // Category is taken from the filename prefix: "fox_01.jpg" -> "fox".
        public int IngestImages(string corpusPath)
        {
            if (!Directory.Exists(corpusPath))
                throw new DirectoryNotFoundException($"Corpus folder not found: {corpusPath}");
            var inserted = 0;
            foreach(var file in Directory.EnumerateFiles(corpusPath, "*jpg"))
            {
                var filename = Path.GetFileName(file);
                var category = filename.Split('_')[0];
                if (_images.Insert(filename, category)) inserted++; 
            }
            return inserted;
        }


        // Seeds a small set of blog posts. The last one (deep sea) has NO matching
        // image in the corpus on purpose, to later prove the "no good match" path.
        public int SeedPosts()
        {
            var seed = new (string slug, string title, string body)[]
            {
            ("red-fox", "The Red Fox",
             "The red fox (Vulpes vulpes) is a small agile predator with a bushy tail and reddish-orange coat, often seen hunting in fields and snow."),
            ("gray-wolves", "Gray Wolves of the North",
             "Gray wolves are large wild canines that live and hunt in packs across northern forests and tundra."),
            ("backyard-rabbits", "Backyard Rabbits",
             "Rabbits are small long-eared herbivores that graze on grass and are common in gardens and meadows."),
            ("owls-at-night", "Owls at Night",
             "Owls are nocturnal birds of prey with large forward-facing eyes and silent flight, hunting small animals after dark."),
            ("elephants-of-africa", "Elephants of Africa",
             "African elephants are the largest land animals, with long trunks and tusks, roaming the savanna in family herds."),
            ("deep-sea-anglerfish", "Creatures of the Deep Sea",
             "In the crushing dark of the deep ocean, the anglerfish lures prey with a glowing bioluminescent light on its head."),
            };

            var inserted = 0;
            foreach (var p in seed)
                if (_posts.Insert(p.slug, p.title, p.body)) inserted++;
            return inserted;
        }
    }
}
