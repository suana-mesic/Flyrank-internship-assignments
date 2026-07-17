# FL-06: Explain It Like You Built It

**Track:** General AI Fluency | **Week:** 5 | **Phase:** Build
**Intern:** Suana Mešić

---

## The piece: why notifications don't live inside the payment transaction

In BookVerse, when a customer pays for an order through Stripe, our webhook handles everything that needs to happen next: reduce the inventory, write a payment summary, change the order status to "Paid", and notify the staff. All of that runs as a single database transaction — which means either everything succeeds, or nothing does.

That's normally a good thing. But notification sending is the problem child.

The code that sends a notification can fail for any reason — the network is slow, SignalR hiccups, something unexpected throws an exception. If that exception happens inside the transaction, the database rolls the whole thing back. Inventory doesn't update. The payment summary doesn't get written. The order status stays unchanged.

But here's the part that makes it a real problem, not just an inconvenience: Stripe has already charged the customer. We didn't handle that part — Stripe did, before our webhook even fired. So from the customer's side, money is gone. From our side, the order looks unpaid. The staff sees an unpaid order and doesn't ship the book. The customer paid and gets nothing.

All because a notification failed.

The fix is a background service. Instead of sending the notification inside the transaction, we just drop it into a queue — a simple in-memory list. Adding something to a queue can't fail in a way that would cause a rollback. The transaction commits: inventory reduced, payment recorded, status updated. Done.

Then, separately, a background service picks items off that queue and sends the notifications on its own time. If one notification fails, it logs the error and moves to the next one. The try/catch sits inside the loop, so one bad send never stops the rest.

The key insight is that the notification and the payment are two different problems with two different failure modes, and tying them together in one transaction means the less important one (telling staff) can break the more important one (recording that someone paid). Separating them means each can fail independently — and the one that matters most is already safe before the risky one even starts.

I used the same pattern again in my capstone project. The `WebhookBackgroundService` there does the same thing: submission gets saved first, notification goes into a bounded channel, background worker drains it. If the webhook receiver is down, the visitor still sees "Hvala!" and the data is in the database. The webhook can catch up later. The visitor's data is never lost because of someone else's problem.
