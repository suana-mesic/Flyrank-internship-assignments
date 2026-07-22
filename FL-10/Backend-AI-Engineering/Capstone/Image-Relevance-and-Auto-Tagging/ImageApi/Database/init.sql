-- pgvector extension: enables the vector(N) column type and similarity search.
CREATE EXTENSION IF NOT EXISTS vector;

-- One row per image in the corpus.
-- status tracks the classification lifecycle: pending -> classified / failed.
CREATE TABLE IF NOT EXISTS images (
    id           SERIAL PRIMARY KEY,
    filename     TEXT NOT NULL UNIQUE,          -- e.g. "fox_01.jpg"
    category     TEXT,                          -- ground-truth from filename, used by the eval
    status       TEXT NOT NULL DEFAULT 'pending',
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Structured output of the vision model, one row per image.
-- attributes is a Postgres text array; confidence drives the "flag, don't guess" rule.
CREATE TABLE IF NOT EXISTS image_tags (
    id           SERIAL PRIMARY KEY,
    image_id     INT NOT NULL UNIQUE REFERENCES images(id),
    subject      TEXT NOT NULL,                 -- main thing in the image, e.g. "red fox"
    category     TEXT NOT NULL,                 -- coarse class, e.g. "animal"
    attributes   TEXT[] NOT NULL DEFAULT '{}',  -- e.g. {"orange fur","snow"}
    caption      TEXT NOT NULL,                 -- one-sentence description (this is what we embed)
    confidence   REAL NOT NULL,                 -- 0..1 from the model
    flagged      BOOLEAN NOT NULL DEFAULT FALSE,-- true when confidence is below our threshold
    model        TEXT,                          -- which vision model produced this
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Embedding of each image's caption. vector(768) = 768 numbers per image.
-- 768 must match the embedding model's output dimension (Gemini text-embedding-004).
CREATE TABLE IF NOT EXISTS image_vectors (
    image_id     INT PRIMARY KEY REFERENCES images(id),
    embedding    VECTOR(768) NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Blog posts we want to illustrate.
CREATE TABLE IF NOT EXISTS posts (
    id           SERIAL PRIMARY KEY,
    slug         TEXT NOT NULL UNIQUE,          -- e.g. "red-fox"
    title        TEXT NOT NULL,
    body         TEXT NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Embedding of each post's text, in the SAME 768-dim space as image_vectors,
-- so we can compare a post to an image directly.
CREATE TABLE IF NOT EXISTS post_vectors (
    post_id      INT PRIMARY KEY REFERENCES posts(id),
    embedding    VECTOR(768) NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- The matching decision for a (post, image) pair.
-- status: suggested | approved | rejected | no_match
-- reason explains a guard rejection (e.g. "tags disagree: wolf vs fox").
CREATE TABLE IF NOT EXISTS pairings (
    id           SERIAL PRIMARY KEY,
    post_id      INT NOT NULL REFERENCES posts(id),
    image_id     INT REFERENCES images(id),     -- nullable: a "no_match" pairing has no image
    similarity   REAL,                          -- cosine similarity of the pair
    status       TEXT NOT NULL DEFAULT 'suggested',
    reason       TEXT,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Cost of every vision / embedding call, so we can track spend per model.
CREATE TABLE IF NOT EXISTS cost_events (
    id            SERIAL PRIMARY KEY,
    operation     TEXT NOT NULL,                -- "vision" | "embedding"
    model         TEXT NOT NULL,
    input_tokens  INT NOT NULL DEFAULT 0,
    output_tokens INT NOT NULL DEFAULT 0,
    cost          NUMERIC(12,6) NOT NULL DEFAULT 0,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Vector indexes: HNSW makes "find the nearest vectors" fast, using cosine distance.
CREATE INDEX IF NOT EXISTS idx_image_vectors_hnsw
    ON image_vectors USING hnsw (embedding vector_cosine_ops);
CREATE INDEX IF NOT EXISTS idx_post_vectors_hnsw
    ON post_vectors USING hnsw (embedding vector_cosine_ops);

-- Ordinary b-tree indexes for the columns we filter/join on often.
CREATE INDEX IF NOT EXISTS idx_images_status   ON images(status);
CREATE INDEX IF NOT EXISTS idx_pairings_post   ON pairings(post_id);
CREATE INDEX IF NOT EXISTS idx_cost_operation  ON cost_events(operation);