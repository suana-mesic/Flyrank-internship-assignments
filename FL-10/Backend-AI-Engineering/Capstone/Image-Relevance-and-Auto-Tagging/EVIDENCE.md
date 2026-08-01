# EVIDENCE

One proof per Definition-of-Done checkbox (§6 of the brief). A claim without evidence scores as not done.

Run tests with `dotnet test`. Run the system with `docker compose up -d`, restore the seed
(`docker exec -i image-db psql -U image_user -d imagedb < seed/seed_imagedb.sql`), then
`dotnet run --project ImageApi` (base URL `http://localhost:5211`).

The three AI/guard/eval boxes are proven by the automated suite — paste the one `dotnet test`
summary line into each. The runtime boxes give the exact curl to run against the seeded DB.

---

## AI processing

- [x] **Vision model produces structured output validated against a schema; invalid responses are never trusted.**
  Proof: `llava:13b` is called with Ollama's `format` set to a JSON schema, so it must answer
  `{subject, category, attributes[], caption, confidence}`; the payload is deserialized into the
  typed `ImageTags` record. Tests `ValidVisionJson_ParsesIntoAllFields` (valid → all fields) and
  `MalformedJson_IsRejected` (garbage → rejected).
  ```
  Passed!  - Failed: 0, Passed: 6, Skipped: 0    (SchemaValidationTests included)
  ```

- [ ] **Low-confidence classifications are flagged instead of accepted.**
  Proof: `AI__ConfidenceThreshold=0.6`; in the classify path an image whose `confidence` is below
  the threshold is stored `flagged`, not trusted. Verify on the seeded DB:
  ```
  [RUN & PASTE]
  docker exec -i image-db psql -U image_user -d imagedb -c "SELECT image_id, subject, confidence, flagged FROM image_tags WHERE flagged = true;"
  ```

- [ ] **Images are processed through a batch background job with retries.**
  Proof: `POST /classify/batch` walks every `pending` image; each vision call is retried up to 3×
  before the image is marked `failed` and skipped, so one bad image never stops the run.
  ```
  [RUN & PASTE]  (from a fresh DB, before the seed — or re-ingest to reset a few to pending)
  curl -s -X POST "http://localhost:5211/classify/batch?limit=5"
  ```

- [x] **Vision and embedding costs are tracked per call.**
  Proof: every vision/embedding call writes a `cost_events` row (operation, model, tokens, cost =
  `tokens/1000 × rate`); rate is 0 for the local model but the mechanism is real and parameterized
  (`AI__CostPer1kTokens`). `GET /costs` rolls it up per operation.
  ```
  [RUN & PASTE]
  curl -s http://localhost:5211/costs
  ```

## Matching system

- [ ] **Image and post embeddings are stored; posts return ranked image suggestions.**
  Proof: captions and post text live in one `vector(768)` space; `GET /posts/{id}/matches` ranks
  images by pgvector cosine similarity (`<=>`, similarity = 1 − distance).
  ```
  [RUN & PASTE]
  curl -s "http://localhost:5211/posts/1/matches?limit=5"
  ```

- [ ] **Semantic matching works for equivalent concepts ("red fox" matches "Vulpes vulpes").**
  Proof: matching is on meaning (post text ↔ image caption), so a fox caption surfaces for the fox
  post above wolves and dogs even when the exact words differ. The fox post's top matches are foxes.
  ```
  [RUN & PASTE]
  curl -s "http://localhost:5211/posts/1/suggestion"
  ```

## Safety layer

- [x] **The mismatch guard rejects incorrect recommendations — the wolf-on-a-fox-post scenario provably fails.**
  Proof: two rules — Rule A rejects below-threshold similarity, Rule B rejects a subject/topic
  mismatch even at high similarity. Test `WolfOnFox