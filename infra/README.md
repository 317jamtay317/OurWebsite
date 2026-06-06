# Deploying ProManager Online to Azure

This site runs as a Container App in the **existing `project-management-env-v2`** environment
(resource group **`ProjectManagement`**, region **East US**) — the same environment as `billingagent`
and the other apps, and it follows the same pattern:

- image pulled from **`registry.hub.docker.com/jamtay317`**;
- **secrets stored inline on the Container App** (no Key Vault, no managed identity), set with `az`
  from the console — never in git or GitHub;
- deployed from your console with `az` (GitHub holds no Azure credentials).

## Conventions (matching billingagent)

| | |
|---|---|
| Environment | `project-management-env-v2` (East US, RG `ProjectManagement`) |
| Registry | `registry.hub.docker.com`, user `jamtay317` |
| Ingress | external, target port `8080` |
| Scale / resources | min 1 / max 1 · 0.5 vCPU / 1Gi |
| Secrets | inline Container App secrets |
| Volumes | none |

> Note: `billingagent` runs with `ASPNETCORE_ENVIRONMENT=Development`. For this public site the create
> command below uses **Production** (Development exposes detailed error pages publicly). Change it if
> you want them identical.

Prerequisites: the SQL server must allow Azure services (SQL server → Networking), and the Docker Hub
repo `jamtay317/promanageronline_website` must be **private**.

---

## One-time create

Sign in, build + push the first image, then create the app. Replace the placeholder values — they go
straight into Azure, never into git.

```powershell
az login
docker login

$repo = 'jamtay317/promanageronline_website'
$tag  = 'v1'
docker build -t "${repo}:$tag" -t "${repo}:latest" .
docker push "${repo}:$tag"; docker push "${repo}:latest"

$conn        = '<your SQL connection string>'
$adminEmail  = 'admin@promanageronline.com'
$adminPw     = '<admin password>'          # >=8 chars: upper, lower, digit, symbol
$dockerToken = '<docker hub read token>'

az containerapp create `
  --name promanageronline-web `
  --resource-group ProjectManagement `
  --environment project-management-env-v2 `
  --image "registry.hub.docker.com/${repo}:$tag" `
  --ingress external --target-port 8080 --transport auto `
  --min-replicas 1 --max-replicas 1 --cpu 0.5 --memory 1.0Gi `
  --registry-server registry.hub.docker.com --registry-username jamtay317 --registry-password $dockerToken `
  --secrets "connectionstring=$conn" "adminemail=$adminEmail" "adminpassword=$adminPw" `
  --env-vars `
    ASPNETCORE_ENVIRONMENT=Production `
    'ASPNETCORE_URLS=http://+:8080' `
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true `
    'ConnectionStrings__SiteDatabase=secretref:connectionstring' `
    'Admin__Email=secretref:adminemail' `
    'Admin__InitialPassword=secretref:adminpassword'
```

On first boot the app applies its EF migrations and seeds the catalogue + admin into your database.
The create command prints the app's FQDN (also: `az containerapp show -n promanageronline-web -g ProjectManagement --query properties.configuration.ingress.fqdn -o tsv`).

---

## Routine deploys

```powershell
az login
docker login
./infra/deploy.ps1              # build + push + roll out a new image
./infra/deploy.ps1 -Tag v2     # explicit tag (default: timestamp)
./infra/deploy.ps1 -SkipBuild  # re-roll the current latest without rebuilding
```

## Updating secrets / config later

```powershell
# change a secret value (e.g. rotate the connection string)
az containerapp secret set -n promanageronline-web -g ProjectManagement --secrets "connectionstring=<new value>"

# turn on SMTP + reCAPTCHA: add the secrets, then reference them as env vars
az containerapp secret set -n promanageronline-web -g ProjectManagement --secrets "smtppassword=<pw>" "recaptchasecret=<key>"
az containerapp update -n promanageronline-web -g ProjectManagement --set-env-vars `
  'Smtp__Host=<host>' 'Smtp__User=<user>' 'Smtp__FromAddress=<from>' 'Smtp__Password=secretref:smtppassword' `
  'Recaptcha__SiteKey=<site key>' 'Recaptcha__SecretKey=secretref:recaptchasecret'
```

A secret change takes effect on the next revision; `az containerapp update` (e.g. a deploy) creates one.

---

## Custom domain (`promanageronline.com` / `www`)

`app.promanageronline.com` **and** `www.promanageronline.com` are currently bound to **billingagent**.
To serve this marketing site at `www` + the apex while leaving the product app on `app.`:

1. Unbind `www` from billingagent:
   ```powershell
   az containerapp hostname delete -n billingagent -g ProjectManagement --hostname www.promanageronline.com
   ```
2. DNS at your registrar — remove the apex→app redirect, then add (get `<fqdn>` / `<verify>` from the
   command below; the env static IP is `172.171.187.196`):

   | Host | Type | Value |
   |---|---|---|
   | `www` | CNAME | `<app fqdn>` |
   | `asuid.www` | TXT | `<customDomainVerificationId>` |
   | `@` (apex) | A | `172.171.187.196` |
   | `asuid` | TXT | `<customDomainVerificationId>` |

   ```powershell
   az containerapp show -n promanageronline-web -g ProjectManagement `
     --query "{fqdn:properties.configuration.ingress.fqdn, verify:properties.customDomainVerificationId}" -o table
   ```
3. Bind to this app with a free managed certificate (per hostname):
   ```powershell
   az containerapp hostname add  -n promanageronline-web -g ProjectManagement --hostname www.promanageronline.com
   az containerapp hostname bind -n promanageronline-web -g ProjectManagement `
     --hostname www.promanageronline.com --environment project-management-env-v2 --validation-method CNAME

   az containerapp hostname add  -n promanageronline-web -g ProjectManagement --hostname promanageronline.com
   az containerapp hostname bind -n promanageronline-web -g ProjectManagement `
     --hostname promanageronline.com --environment project-management-env-v2 --validation-method TXT
   ```

The nav already includes a **Customer Portal** link to `https://app.promanageronline.com` and a
**Sign in** link to the site's own admin.

---

## Local development is unaffected

`docker compose up --build` still runs the app against its own SQL Server container exactly as before.
This Azure path is entirely separate.
