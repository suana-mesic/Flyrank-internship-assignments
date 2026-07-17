# PF-04: Personal Website Live on the FlyRank Domain

**Track:** General AI Fluency | **Week:** 5 | **Phase:** Build
**Intern:** Suana Mešić

---

## Live URL

**https://github.com/suana-mesic/suana-mesic.github.io/tree/f3** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.

**https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

Hosted on GitHub Pages. HTTPS is automatic — GitHub provisions a certificate through Let's Encrypt. The site loads over a secure connection with no manual setup.

The site contains: positioning (who I am, what I do), a BookVerse case study, and working links to LinkedIn, GitHub, CV (PDF), and a Calendly booking link. One page, plain HTML and CSS, no framework.

---

## DNS walkthrough

### What is DNS

DNS is a phonebook. You type `suana-mesic.github.io` into the browser — but the browser needs an IP address to actually connect. DNS is the system that translates the name into the address.

### What happens when someone types my URL

1. **The browser checks its own cache.** If it visited this site recently, it already knows the IP and skips straight to connecting.

2. **If not cached, the browser asks the operating system, which asks a resolver.** The resolver is usually run by the user's internet provider. Its job is to find the answer by asking other servers.

3. **The resolver asks the root nameserver:** "Who handles `.io` domains?" The root says: "Ask the `.io` nameserver, here's its address."

4. **The resolver asks the `.io` nameserver:** "Who handles `github.io`?" It says: "Ask GitHub's nameserver."

5. **The resolver asks GitHub's nameserver:** "What is the IP for `suana-mesic.github.io`?" GitHub's nameserver looks it up and returns the IP address (e.g. `185.199.108.153`).

6. **The resolver sends the IP back to the browser.** The browser connects to that IP and loads the page. The whole chain took milliseconds.

### What a CNAME record is

A CNAME record is an alias — it says "this name is actually another name." Instead of pointing directly at an IP address (which is what an A record does), a CNAME points at a different domain name, and the resolver follows the chain.

### What mine will look like

When my FlyRank subdomain is provisioned, Ops will create:

```
suana.flyrank.ai  CNAME  suana-mesic.github.io
```

This means: anyone who types `suana.flyrank.ai` gets redirected (at the DNS level, invisibly) to `suana-mesic.github.io`, which then resolves to GitHub's IP. The browser never sees the redirect — it just gets the page.

**What I do on my side:** in the GitHub Pages settings, I add `suana.flyrank.ai` as a custom domain. GitHub then knows that when a request arrives for that domain, it should serve my site. Without this step, GitHub would reject the request because it wouldn't recognise the domain as belonging to any of its Pages sites.

**What actually changes:** nothing about the site itself. Same files, same host, same HTTPS. The custom domain is a pointer, not a migration. Both URLs work — the old one doesn't break.

### The full chain after the subdomain

1. Someone types `suana.flyrank.ai`
2. Resolver finds the CNAME → `suana-mesic.github.io`
3. Resolver follows the CNAME and finds the A record → `185.199.108.153`
4. Browser connects to GitHub's server at that IP
5. GitHub sees the `Host: suana.flyrank.ai` header, matches it to my repo, serves the page
6. HTTPS certificate covers both domains — GitHub provisions it automatically

---

## What I still need to do on LinkedIn and CV

- **LinkedIn:** add `suana-mesic.github.io` to the Contact info section (Edit profile → Contact info → Website). Swap it for `suana.flyrank.ai` when the subdomain is provisioned.
- **CV:** add the URL to the header alongside email and GitHub. Same swap later.

---

## Files

- `README.md` — this document (DNS walkthrough)
- `CV.pdf` — my CV, also linked from the portfolio page
