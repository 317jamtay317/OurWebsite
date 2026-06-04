# ProManager Online — marketing website

A fast, dependency-free **static website** for ProManager Online, built to clearly
present the business — subscription software products plus custom development —
and to support a payment-processor (Stripe) account review.

It is plain HTML, CSS and a tiny bit of JavaScript. There is **no build step**.
It is served by **nginx in Docker**.

The business sells two subscription products — **Workflows.AI** (all-in-one business
management for small teams) and **Air Compliance Record Keeping** (air-emissions
compliance for hot-mix asphalt plants) — and also builds custom applications, billed
per project milestone.

---

## Run it with Docker

From this folder (`D:\web-sites\website`):

```bash
docker compose up --build
```

Then open **http://localhost:8080**.

Other useful commands:

```bash
docker compose up -d --build   # run in the background
docker compose down            # stop and remove the container
docker compose logs -f         # follow logs
```

To change the port, edit the `ports` mapping in `docker-compose.yml`
(e.g. `"3000:80"` → http://localhost:3000).

### Without Docker (optional)

It's just static files, so you can also preview with any static server:

```bash
# Python (already installed on this machine)
cd public
python -m http.server 8080
```

---

## Project structure

```
website/
├─ public/                 # the entire website (this is what gets served)
│  ├─ index.html           # Home
│  ├─ products.html        # Product overview (both products)
│  ├─ workflows.html       # Product: Workflows.AI (business management ERP)
│  ├─ air-compliance.html  # Product: Air Compliance Record Keeping (asphalt plants)
│  ├─ services.html        # Custom development + milestone billing
│  ├─ pricing.html         # Workflows.AI tiers + Air Compliance (quote) + project pricing
│  ├─ docs.html            # Documentation hub: getting-started + how-tos per product
│  ├─ about.html           # Who we are / business model
│  ├─ contact.html         # Contact form + details
│  ├─ faq.html             # Clarifies what we are / are NOT (for review)
│  ├─ terms.html           # Terms of Service
│  ├─ privacy.html         # Privacy Policy
│  ├─ refunds.html         # Refunds & Cancellation
│  ├─ 404.html             # Not-found page
│  ├─ robots.txt, sitemap.xml
│  └─ assets/
│     ├─ css/styles.css    # All styling (mobile-first design system)
│     ├─ js/main.js        # Mobile nav toggle + active-link highlighting
│     └─ img/favicon.svg   # Logo / favicon
├─ Dockerfile              # nginx:alpine image
├─ default.conf            # nginx config (clean URLs, gzip, security headers)
├─ docker-compose.yml      # one-command local run
├─ .dockerignore
└─ README.md
```

Editing content is just editing the relevant `.html` file. Colours, spacing and
fonts are controlled by CSS custom properties at the top of
`public/assets/css/styles.css` (the `:root { … }` block).

---

## ✅ Before you go live — fill in these placeholders

Search the project for each item and replace it. These matter for the Stripe
review, so use real, consistent details.

| Placeholder | Where | What to put |
|---|---|---|
| `[Owner Name]` | footer of every page, `about.html`, `contact.html` | Your legal name (the sole proprietor). Should match your Stripe account. |
| `[City, Country]` | `about.html`, `contact.html` | Your business location. A full address helps with Stripe. |
| `[State / Country]` | `terms.html` (governing law) | Your governing-law jurisdiction. |
| `support@promanageronline.com` | everywhere | Set up this mailbox (or forward it). A domain email beats a Gmail for approval. |
| Contact form `action` | `contact.html` | Connect a form service (e.g. Formspree) — see below. The email link works in the meantime. |
| Workflows.AI prices ($29 / $79 / $149) | `pricing.html` | Confirm or replace with your real plan prices. |
| Air Compliance pricing | `pricing.html` | Currently "per facility — request a quote". Add concrete pricing if/when you set it. |

### Product copy — worth a sanity check

Product descriptions were written from a review of the source code, but a couple of
details are worth your confirmation before launch:

- **Workflows.AI** is described by its real features (customers/CRM, quotes, invoicing,
  inventory, accounting, workflow automation). It's positioned for small contractors/
  teams. The ".AI" automation engine is real; there are **no LLM/AI claims** beyond that
  — keep it that way unless/until those features ship.
- **Air Compliance Record Keeping** names Indiana's **IDEM** specifically and references
  multi-state support. If you actively support other states (e.g. Michigan), name them;
  if not yet, leave it as "states including IDEM".

### Connecting the contact form

The form currently posts to a placeholder. The quickest option is
[Formspree](https://formspree.io): create a form, then set
`action="https://formspree.io/f/your-real-id"` in `public/contact.html`.
If you use a **different** provider, also update the `form-action` directive in
the Content-Security-Policy inside `default.conf`.

---

## Why this site supports the Stripe review

The account was flagged as a regulated/restricted business. This site is written
to make the real business model unmistakable:

- **Clear products & services** — two concrete subscription products (Workflows.AI and
  Air Compliance Record Keeping) and custom development, described plainly.
- **Transparent pricing** — recurring subscription tiers, per-facility pricing, and
  project work billed per delivered milestone.
- **An explicit "what we are NOT" statement** — not a lender, no financing/credit,
  no escrow, no third-party payment processing — repeated on the Home, About,
  Services and FAQ pages, mirroring the description submitted to Stripe.
- **Required policy pages** — Terms, Privacy, and Refunds & Cancellation.
- **Real contact path** — support email + contact form + business identity.

> ⚠️ **Legal note:** `terms.html`, `privacy.html` and `refunds.html` are general
> templates for convenience, **not legal advice**. Have them reviewed for your
> jurisdiction before relying on them.

---

## Deploying

- **Docker anywhere** — build the image and run it on any host/VPS that runs
  Docker (the same `docker compose up` works).
- **Free static hosting** — because it's static, you can also drop the contents
  of `public/` onto Cloudflare Pages, Netlify or GitHub Pages at no cost, then
  point `promanageronline.com` at it. The Docker setup remains available for
  local preview or self-hosting.
```
