CREATE TABLE IF NOT EXISTS items (
    id             UUID PRIMARY KEY,
    name           TEXT NOT NULL,
    created_at_utc TIMESTAMPTZ  NOT NULL
);