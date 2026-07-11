# FL-02: The Prompt Ladder

**Track:** General AI Fluency
**Week:** 2 | **Phase:** Foundations
**Intern:** Suana Mešić — Junior Backend Developer

---

## The task

Start with a genuinely weak prompt, then improve it across five versions — each adding exactly one layer — and compare every output against the previous one. Six runs total, so I can see exactly which single change earned its place.

**Base topic:** explaining Clean Architecture (a concept I actually need for interviews).

---

## Baseline

**Prompt:**
> Explain Clean Architecture

**Output (excerpt):**
> A textbook explanation: the Dependency Rule, the four concentric rings (Entities, Use Cases, Interface Adapters, Frameworks & Drivers), dependency inversion, benefits, tradeoffs, and related patterns (Hexagonal, Onion). Accurate but written for no one in particular; ends by offering a folder structure or code example.

This is the baseline — it answers *a* question, just not *mine*.

---

## Version 1 — layer added: GOAL

**Prompt:**
> Explain Clean Architecture so I can describe it clearly in a job interview for a junior backend role.

**Output (excerpt):**
> Reframes around interviews: gives a one-sentence definition to memorize, flags the single line to actually say ("dependencies point inward"), explains dependency inversion with a repository example, lists likely follow-ups ("How is it different from MVC?"), and tells me to mention the tradeoff to show maturity.

**Four notes:**
- **What changed in the prompt:** added the goal — why I need it (a junior backend interview).
- **What improved in the output:** stopped being a textbook dump and started coaching me on what to *say* and which line matters most.
- **What still failed:** still generic to "a junior role" — no connection to my actual experience or stack.
- **What I'd try next:** tell it I've already built something, so it skips the basics.

---

## Version 2 — layer added: AUDIENCE (my level)

**Prompt:**
> Explain Clean Architecture so I can describe it clearly in a job interview for a junior backend role. I am a computer science student who has built one ASP.NET Core project using it, so skip the absolute basics.

**Output (excerpt):**
> Now assumes .NET and maps to a real solution structure (Domain / Application / Infrastructure / API). Introduces the compile-time-vs-runtime distinction and the DI composition root (`AddScoped<IOrderRepository, OrderRepository>()`), and warns against overclaiming "easy database swaps."

**Four notes:**
- **What changed in the prompt:** added audience/level — that I've built one ASP.NET Core project and don't need the basics.
- **What improved in the output:** it raised the ceiling — .NET-specific detail and the compile-time-vs-runtime point that separates understanding from template-cloning.
- **What still failed:** the example was still a generic order API, not my project — nothing I could point to as mine.
- **What I'd try next:** name my actual project and its layers.

---

## Version 3 — layer added: CONTEXT (my real project)

**Prompt:**
> Explain Clean Architecture so I can describe it clearly in a job interview for a junior backend role. I am a CS student who built an ASP.NET Core e-commerce backend (BookVerse) using it. Relate the explanation to layers like Domain, Application, Infrastructure, and API.

**Output (excerpt):**
> Everything is anchored in BookVerse: Domain = Book/Order/Cart entities and rules, Application = PlaceOrderHandler and interfaces like IBookRepository/IPaymentGateway, Infrastructure = EF Core + Stripe implementations, API = thin controllers. Gives a full order-placement walkthrough and BookVerse-specific follow-up answers (where validation goes, why the interface lives in Application).

**Four notes:**
- **What changed in the prompt:** added real context — my project name, domain, and the .NET layer names.
- **What improved in the output:** the answer became one only I could give — tied to code I actually wrote, which is what convinces an interviewer.
- **What still failed:** it was long and unstructured — a wall of good material with no fixed shape, too much to say out loud.
- **What I'd try next:** force a fixed format so it's deliverable.

---

## Version 4 — layer added: FORMAT

**Prompt:**
> Explain Clean Architecture for a junior backend interview. Context: I built an ASP.NET Core e-commerce backend (BookVerse) with Domain, Application, Infrastructure, and API layers. Format the answer as: (1) a two-sentence definition, (2) what each layer does, (3) the dependency rule, (4) one concrete example from an e-commerce app.

**Output (excerpt):**
> Same BookVerse content, now in four clean numbered sections: a two-sentence definition, one line per layer, the dependency rule as a single paragraph, and one order-placement example. Much easier to follow and to rehearse.

**Four notes:**
- **What changed in the prompt:** added a fixed format — four numbered parts.
- **What improved in the output:** structure. The same information became scannable and rehearsable instead of a wall of text.
- **What still failed:** still a bit long and slightly formal — a few phrases I wouldn't naturally say out loud.
- **What I'd try next:** add a length limit and a "sounds spoken, no buzzwords" constraint.

---

## Version 5 — layer added: CONSTRAINTS + QUALITY BAR

**Prompt:**
> Explain Clean Architecture for a junior backend interview. Context: I built an ASP.NET Core e-commerce backend (BookVerse) with Domain, Application, Infrastructure, and API layers. Format: (1) two-sentence definition, (2) what each layer does, (3) the dependency rule, (4) one concrete e-commerce example. Keep it under 250 words, no buzzwords, and make it sound like something I could say out loud in an interview without reading a script.

**Output (excerpt):**
> A tight, under-250-word answer in the same four parts, but now in plain spoken language: "Application is where the actual actions live... it also declares the interfaces it needs... without building them." No jargon, nothing I'd stumble over reading aloud. This is the version I'd actually use.

**Four notes:**
- **What changed in the prompt:** added constraints (under 250 words) and a quality bar (no buzzwords, must sound spoken).
- **What improved in the output:** it became genuinely deliverable — short, plain, and natural to say in a room without a script.
- **What still failed:** honestly little; if anything it's still close to the 250-word ceiling for a true 60-second answer.
- **What I'd try next:** a separate 60-second variant for when the interviewer wants the short version.

---

## Honest moment

Not every layer helped equally. The **CONTEXT** layer (Version 3) was the single biggest jump — it turned a good generic answer into one only I could give. **FORMAT** (Version 4) added no new information at all; it only made the existing information usable. That taught me the order matters: if I'd added format before context, the answer would have been well-structured but still generic. The **AUDIENCE** layer (Version 2) helped less than I expected — it mostly just skipped the intro paragraph.

---

## Final reusable prompt

Cleaned up so someone else on my track could take it and use it without me in the room:

> Explain [CONCEPT] for a [junior/mid] [role] interview.
> Context: [one line on a real project you built, its domain, and the layers/stack involved].
> Format the answer as: (1) a two-sentence definition, (2) what each part does, (3) the single most important rule or principle, (4) one concrete example from my project.
> Keep it under 250 words, no buzzwords, and make it sound like something I could say out loud in an interview without reading a script.
