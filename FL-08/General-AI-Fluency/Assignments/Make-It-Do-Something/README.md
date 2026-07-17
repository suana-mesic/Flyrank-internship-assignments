# FL-08: Make It Do Something

**Track:** General AI Fluency | **Week:** 8 | **Phase:** Submit
**Intern:** Suana Mešić

---

## Live URL

- **https://github.com/suana-mesic/suana-mesic.github.io/tree/f5** — The version built specifically for this assignment. You can view it by cloning the repository and double-clicking index.html.
- **https://suana-mesic.github.io** — The live production version, which updates depending on how many tasks I have completed.

---

## The feature

A **contact form** at the bottom of the portfolio. A visitor types their name, email, and message, clicks "Send message", and the message arrives in my inbox. One feature, working end to end.

---

## How it works — plain-words explainer

A static website like mine (plain HTML on GitHub Pages) has no backend — there's no server running my code that can receive data and do something with it. GitHub Pages only serves files; it can't process a form submission.

That's the problem a contact form creates: the visitor fills in data, but there's nowhere on my site to send it.

**Formspree** fills that gap. It gives me a URL — an endpoint — that acts as the backend I don't have. Here's the data flow:

1. The visitor fills in the form and clicks "Send message."
2. The browser sends the form data (name, email, message) as an HTTP POST request to `https://formspree.io/f/mojgerkq`. That URL is written in the form's `action` attribute — that's the only line that connects my static page to an external service.
3. Formspree's server receives the POST, validates it (checks for spam, confirms the fields), and forwards it to my email address. I registered that email when I created the form on formspree.io.
4. Formspree redirects the visitor to a "thank you" page, so they know the message went through.
5. I get an email with the visitor's name, email, and message. I can reply to them directly from my inbox.

**What a backend is, in this context:** a server that receives data from a browser and does something with it — stores it, sends an email, runs a calculation. My portfolio doesn't have one. Formspree is a third-party backend that does exactly one thing: receive form data and forward it as email. I don't write or host any server code for this.

**What I could do instead:** build my own backend (like the ASP.NET Core APIs I build for my internship) that receives the POST and sends the email itself. That would mean hosting a server, paying for it, and maintaining it — all for one contact form. Formspree's free tier does the same job with one line of HTML.

---

## Evidence

Sent a test submission from the live site. Screenshot of the email arriving in my inbox is included.

---

## Files

- `README.md` — this explainer
- `test-submission.jpg` — screenshot of the test email received in my inbox
