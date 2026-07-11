# FL-02: Frame It as Cases — Work That Speaks for Itself

**Track:** General AI Fluency
**Week:** 2 | **Phase:** Foundations
**Intern:** Suana Mešić — Junior Backend Developer

---

## Voice Card

> **direct, precise, technical but clear, honest, no buzzwords**

Every case below is written in this voice: short sentences, real decisions, no inflated results.

---

## Case Study: BookVerse — Reliable Payment Notifications

**Audience:** a hiring manager or technical lead at an established company.

### The problem
In BookVerse, the Stripe payment webhook handler ran everything inside a single database transaction — reducing inventory, recording the successful payment, and sending a notification to staff (employee, manager, admin) that a customer had paid. Putting the notification inside the transaction was the flaw: if the notification failed to send, the handler threw, the whole transaction rolled back, and inventory and payment records were never saved — even though the customer had already seen "payment successful" on the UI. An external call that could fail was deciding whether a payment was allowed to persist.

### What I did
I moved the notification out of the transaction using a Transactional Outbox pattern. Inside the payment transaction, the order is written to an outbox queue (FIFO) instead of notifying directly — so the message is committed atomically with the payment, or rolled back with it. A separate background service reads the queue one message at a time and sends the notification over SignalR, which gives real-time server-to-client delivery over WebSocket. Payment and notification are now decoupled: the transaction commits as soon as the payment is recorded, and delivery happens independently afterward.

### What came of it
The payment path became reliable. A failed notification no longer rolls back a real payment: inventory is reduced, the payment is recorded, and staff still get notified — but a delivery failure can never undo a completed purchase. **What I would do differently:** the background service currently passes through the queue once with no retry. Adding a retry (and keeping failed messages in the outbox until they succeed) is the natural next step, and is one of the real strengths of the outbox pattern I have not fully used yet.

---

## Before / After

**Generic AI line:**
> "Leveraged a robust, scalable background architecture to deliver seamless, real-time notifications and ensure a flawless payment experience."

**My edited version:**
> "Moved notifications out of the payment transaction so a failed message can't roll back a real payment. Inventory and payment now persist independently of notification delivery."

---

## Bio & Contact

**Bio:** I am a junior backend developer who builds systems where the important thing keeps working even when a small part fails. I care about clean architecture and making sure the data tells the truth.

**Contact / CTA:** Want to see how I build? Visit my GitHub.
