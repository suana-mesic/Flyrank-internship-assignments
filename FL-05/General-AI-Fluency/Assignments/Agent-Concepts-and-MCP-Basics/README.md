# FL-05: Agent Concepts and MCP Basics

**Track:** General AI Fluency | **Week:** 4 | **Phase:** Build (core)
**Intern:** Suana Mešić

---

## Workflow vs agent

The whole distinction comes down to one question: **who decides the next step?**

In a workflow, the steps are fixed before the task starts. Someone wrote down the order — a developer in code, or me by hand — and the model fills in each step without choosing which steps run or in what sequence. Anthropic's essay puts it as systems where models and tools are orchestrated through predefined code paths.

An agent decides for itself. It runs in a loop: take an action, look at what came back, decide what to do next, repeat. Crucially, it also decides **when it's finished** — nobody tells it "you have four steps." The essay's phrasing is that agents dynamically direct their own processes and tool usage.

My practical test: if I can draw the flowchart before the task runs, it's a workflow. If the flowchart depends on what the model finds while running, it's an agent.

Neither is better. The essay is blunt that you should start simple and only add agency when it demonstrably improves the outcome — agency costs tokens, time, and predictability.

## My FL-04 pipeline is a workflow

No ambiguity here. My study-notes pipeline runs gather → synthesize → format → review. Always those four, always in that order. A lecture on databases and a lecture on UML take an identical path; nothing inspects the material and decides anything.

It's a **prompt chain** — each step's output is the next step's input. And the orchestrator isn't code, it's me: I copy NotebookLM's summary into a Claude Project by hand. Without me sitting there, it doesn't run. That's as far from an agent as it gets — and it's fine, because the steps genuinely don't need to change.

## What MCP is

MCP is an open protocol for letting a model reach things outside the chat window. The docs call it a USB-C port for AI applications, and that's the right idea: one standard plug, many devices. Without it, every model-plus-tool combination is a custom integration; with it, any MCP client can talk to any MCP server.

The pieces: the **host** is the app (Claude Desktop), the **server** is a small program that exposes some capability, and the client sits between them.

### The three primitives

What confused me at first is that all three come from the server, not from my chat. Attaching a PDF to a conversation isn't MCP — it's just an upload.

| Primitive | Who controls it | What it is |
|---|---|---|
| **Tools** | the model | an action the model calls on its own — `read_file`, `search_files` |
| **Resources** | the application | data the server offers, pulled into context |
| **Prompts** | the user | a ready-made template the user triggers |

The line that finally made it click: **a tool is an action, a resource is data.** `read_file` is a tool. The file it returns is a resource. And a tool is invoked by the model mid-conversation — it isn't me pasting code in.

## What I ran

I connected the Filesystem MCP server to Claude Desktop and pointed it at my capstone folder. Three tasks that plain chat could not have done, because plain chat cannot see my disk:

1. **Read a file** — "read `PostgresWidgetRepository.cs` and tell me which SQL statements filter by `tenant_id`." It found five that do, and correctly flagged the one that doesn't (`GetActiveByIdAsync`, the public config lookup) as worth double-checking.
2. **Search across files** — "find every file that mentions `tenantId`." It walked the tree and read candidates, returning 11 files, and noted that `widget.js` and the customer page contain none — correct, since public-facing code never sees tenant info.
3. **Write a file** — it created `MCP-TEST.md` in my capstone folder listing my repository interfaces. This is the strongest evidence: chat can describe a file, but only a tool can put one on my disk.

One thing I hadn't expected: the server only sees the folder I granted it. It couldn't reach the rest of my drive, and each action asked for approval. The permission boundary is part of the protocol, not an afterthought.

### Evidence

- `mcp-connector-config.png` — the Filesystem connector: enabled, its allowed directory, and its tool permissions. A built-in file feature has no tool list; this screen is what makes it MCP.
- `task1-read-file.png` — reading the repository file (`Search Files`, `Read Text File`).
- `task2-search-files.png` — searching the tree (`Read Multiple Files`).
- `task3-write-file.png` — creating `MCP-TEST.md`.
- `task3-file-on-disk.png` — the file in Explorer afterwards, timestamped. Chat can describe a file; only a tool leaves one behind.

## What would make my pipeline an agent

One concrete upgrade: **an evaluator-optimizer loop with filesystem tools.**

Right now, step 4 generates practice questions and stops. An agent version would, after producing the notes, re-read the source itself, compare its notes against it, and ask *"did I miss a concept the lecture emphasized?"* If yes, it revises and checks again — looping until it passes its own review. I never say how many passes. It decides when the notes are good enough, which is exactly the "decides when it's done" property that separates agent from workflow.

MCP is what makes that possible. With filesystem tools, it opens the lecture PDF itself instead of waiting for me to upload it, and it can write the finished notes to disk. My copy-paste job disappears — and with it, the reason my pipeline is a workflow at all.

Watching the MCP session was the clearest example. When the recursive search didn't return what it expected, it listed the tree and searched each subfolder instead. Nobody told it to. That small adaptation — noticing a result was wrong and choosing a different approach — is the thing my pipeline has never done once.
