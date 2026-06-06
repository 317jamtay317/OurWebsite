using Microsoft.EntityFrameworkCore;
using ProManagerOnline.Site.Domain.Content;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence;

/// <summary>Seeds the catalogue with the initial products and their documentation when the database is empty.</summary>
public static class SiteDbSeeder
{
    /// <summary>
    /// Adds the initial published products — and a starter set of documentation articles for
    /// Workflows.AI — if the catalogue is currently empty.
    /// </summary>
    /// <param name="context">The site database context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task SeedAsync(SiteDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedAboutPageAsync(context, cancellationToken);

        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var workflows = Product.CreateDraft(
            Slug.Create("workflows"),
            "Workflows.AI",
            "Business management",
            "An all-in-one platform that brings customers, quotes, invoicing, inventory and accounting together — with workflow automation for the routine work.");
        workflows.AddPlan(
            "Solo", "For an owner-operator getting organised.", Money.Create(29m, Currency.Usd), BillingPeriod.Monthly,
            ["1 user", "Customers & CRM", "Quotes & invoicing", "Email support"]);
        var team = workflows.AddPlan(
            "Team", "For small contractors and crews.", Money.Create(79m, Currency.Usd), BillingPeriod.Monthly,
            ["Up to 5 users", "Everything in Solo", "Inventory management", "Workflow automation", "Priority email support"]);
        workflows.AddPlan(
            "Business", "For teams that have outgrown five seats.", Money.Create(149m, Currency.Usd), BillingPeriod.Monthly,
            ["Unlimited users", "Everything in Team", "Accounting & reporting", "API access", "Phone & email support"]);
        workflows.FeaturePlan(team);
        workflows.Publish();

        var airCompliance = Product.CreateDraft(
            Slug.Create("air-compliance"),
            "Air Compliance Record Keeping",
            "Environmental compliance",
            "Purpose-built for hot-mix asphalt plants: log daily production, track permit limits automatically, and generate the air-emissions reports your state agency expects.");
        airCompliance.MakeQuoteBased();
        airCompliance.Publish();

        context.Products.Add(workflows);
        context.Products.Add(airCompliance);
        context.DocArticles.AddRange(WorkflowsDocumentation(workflows.Id));
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAboutPageAsync(SiteDbContext context, CancellationToken cancellationToken)
    {
        if (await context.AboutPages.AnyAsync(cancellationToken))
        {
            return;
        }

        context.AboutPages.Add(AboutPage.Create(
            "About ProManager Online",
            """
            ProManager Online builds practical business software for small companies — from
            subscription products like Workflows.AI to purpose-built compliance tools and custom
            application development.

            We're an independent software business focused on doing a few things well: shipping
            software that's dependable, easy to live with, and genuinely useful day to day.

            ## What we do

            - **Subscription products** — ready-to-use apps you can sign up for and start using today.
            - **Custom development** — tailored applications built around how your business actually works.
            - **Ongoing support** — we stand behind what we ship.

            Have a question or a project in mind? [Get in touch](/contact) — we'd love to hear from you.
            """));

        await context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<DocArticle> WorkflowsDocumentation(ProductId productId) =>
    [
        Published(productId, "welcome", "Welcome to Workflows.AI", "Getting started", 0,
            """
            # Welcome to Workflows.AI

            Workflows.AI brings your customers, quotes, invoicing, inventory and accounting
            together in one place — with automation for the routine work.

            This documentation walks you through everything from your first sign-in to the
            everyday tasks that keep your business moving.

            ## What you'll find here

            - **Getting started** — set up your account and learn your way around.
            - **How-to guides** — step-by-step instructions for everyday tasks.

            > Tip: use the sidebar on the left to jump between topics at any time.
            """),
        Published(productId, "create-your-account", "Create your account", "Getting started", 1,
            """
            # Create your account

            1. Go to the sign-up page and enter your business email address.
            2. Choose a password and confirm your email.
            3. Add your business details — name, address and logo.

            Once your account is ready you'll land on the **dashboard**, your home base for
            everything that follows.
            """),
        Published(productId, "navigate-the-dashboard", "Navigate the dashboard", "Getting started", 2,
            """
            # Navigate the dashboard

            The dashboard is organised into a few key areas:

            | Area | What it's for |
            | --- | --- |
            | Sales | Quotes, invoices and payments |
            | Customers | Your contacts and their history |
            | Inventory | Products, stock levels and suppliers |

            Use the top navigation to move between areas. Your most recent activity always
            appears front and centre.
            """),
        Published(productId, "create-and-send-a-quote", "Create and send a quote", "How-to guides", 3,
            """
            # Create and send a quote

            1. Go to **Sales > Quotes** and choose **New quote**.
            2. Pick a customer, or create one on the spot.
            3. Add line items — products pull straight from your inventory.
            4. Review the total, then choose **Send** to email it to your customer.

            Your customer can accept the quote online, which moves it straight into an order.
            """),
        Published(productId, "convert-a-quote-to-an-invoice", "Convert a quote to an invoice", "How-to guides", 4,
            """
            # Convert a quote to an invoice

            When a quote is accepted you can turn it into an invoice in one step:

            1. Open the accepted quote.
            2. Choose **Convert to invoice**.
            3. Confirm the payment terms and due date.

            The invoice keeps a link back to the original quote, so your records stay tidy.
            """),
        Published(productId, "track-inventory", "Track inventory", "How-to guides", 5,
            """
            # Track inventory

            Workflows.AI keeps stock levels up to date as you quote, sell and restock.

            - **Automatic adjustments** — selling an item reduces its stock automatically.
            - **Low-stock alerts** — get notified before you run out.
            - **Supplier records** — reorder with the details already on file.

            Head to **Inventory > Products** to see everything you have on hand.
            """),
    ];

    private static DocArticle Published(
        ProductId productId, string slug, string title, string section, int position, string body)
    {
        var article = DocArticle.CreateDraft(productId, Slug.Create(slug), title, section, body, position);
        article.Publish();
        return article;
    }
}
