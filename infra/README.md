# Deploying ProManager Online to Azure

The site runs on **Azure Container Apps** against an **externally hosted SQL database**. You deploy it
from your console with **`az`** + the [`deploy.ps1`](deploy.ps1) script — GitHub is not involved in
deployment and holds no Azure credentials. All runtime secrets live in **Azure Key Vault**; the
Container App reads them via a **managed identity**, so no secret ever passes through git or the
command line.

## What the deploy creates (resource group `ProjectManagement`)

| Resource | Purpose |
|---|---|
| Log Analytics workspace | Container logs |
| Storage account + 2 file shares | Persists uploaded doc media and Data Protection keys across restarts |
| Container Apps environment | Hosts the app; scales to zero when idle |
| User-assigned managed identity | Lets the app read its secrets from Key Vault |
| Container App | The running site, pulling the private Docker Hub image |

The **database and Key Vault are not created here** — you provide them (steps below). Estimated cost
for what *is* created: **~$10–20/month** at low traffic (Container App + storage + logs). Key Vault is
effectively free (no monthly fee; ~$0.03 per 10k secret reads).

---

## One-time setup (`az` in the console)

Sign in first: `az login`. The account needs **Owner** on the resource group (so the deploy can grant
the managed identity access to the vault).

### 1. Resource group + Key Vault

```powershell
az group create -n ProjectManagement -l eastus2

az keyvault create -n kv-promanageronline -g ProjectManagement -l eastus2 `
  --enable-rbac-authorization true
```

### 2. Let yourself manage the vault's secrets

```powershell
$me = az ad signed-in-user show --query id -o tsv
$vaultId = az keyvault show -n kv-promanageronline -g ProjectManagement --query id -o tsv
az role assignment create --assignee $me --role "Key Vault Secrets Officer" --scope $vaultId
```

### 3. Add the secrets (values go straight into Azure — never into git or GitHub)

```powershell
az keyvault secret set --vault-name kv-promanageronline -n sql-connection-string  --value '<your SQL connection string>'
az keyvault secret set --vault-name kv-promanageronline -n admin-email            --value 'admin@promanageronline.com'
az keyvault secret set --vault-name kv-promanageronline -n admin-initial-password --value '<admin password>'
az keyvault secret set --vault-name kv-promanageronline -n dockerhub-token        --value '<Docker Hub read token>'

# Optional — only if you deploy with -EnableSmtp / -EnableRecaptcha:
# az keyvault secret set --vault-name kv-promanageronline -n smtp-password    --value '<smtp password>'
# az keyvault secret set --vault-name kv-promanageronline -n recaptcha-secret --value '<recaptcha secret>'
```

The admin password must satisfy the policy: **≥8 chars with upper, lower, digit, and a symbol**.

### 4. Let the Container App reach your database

On the SQL server that hosts the database, enable **"Allow Azure services and resources to access this
server"** (Azure portal → SQL server → Networking). Without this the app cannot connect.

### 5. Make sure the Docker Hub repo is private

`jamtay317/promanageronline_website` should be **private**; the `dockerhub-token` lets the Container
App pull it.

---

## Deploy

Sign in to both, then run the script from the repo root:

```powershell
az login
docker login                       # Docker Hub, to push the image
./infra/deploy.ps1
```

It builds the image, pushes it to Docker Hub, deploys the Bicep, and prints the live URL
(e.g. `https://promgr-web.<region>.azurecontainerapps.io`). On first boot the app applies its EF
migrations and seeds the catalogue + admin into your database.

Useful switches:

```powershell
./infra/deploy.ps1 -Tag v1                       # explicit image tag (default: timestamp)
./infra/deploy.ps1 -EnableSmtp -EnableRecaptcha  # turn on contact-form email + reCAPTCHA
./infra/deploy.ps1 -SkipBuild                    # redeploy infra without rebuilding the image
```

> Rotating a secret? Update it in Key Vault and restart the app to pick it up:
> `az containerapp revision restart -g ProjectManagement -n promgr-web --revision <name>`
> (or just redeploy).

---

## Custom domain (`promanageronline.com`)

Today the apex redirects to `app.promanageronline.com`. To point it at this marketing site instead
(leaving `app.promanageronline.com` — your product app — untouched):

1. **Remove the apex → app redirect** at your domain registrar/DNS.
2. Get the values you'll need:
   ```powershell
   az containerapp show -g ProjectManagement -n promgr-web `
     --query "{fqdn:properties.configuration.ingress.fqdn, verify:properties.customDomainVerificationId}" -o table
   az containerapp env show -g ProjectManagement -n promgr-cae --query properties.staticIp -o tsv
   ```
3. Add DNS records at your registrar:

   | Host | Type | Value |
   |---|---|---|
   | `www` | CNAME | the Container App `fqdn` |
   | `asuid.www` | TXT | the `customDomainVerificationId` |
   | `@` (apex) | A | the environment `staticIp` |
   | `asuid` | TXT | the `customDomainVerificationId` |

4. Bind the hostnames with a free managed certificate (run per hostname):
   ```powershell
   az containerapp hostname add  -g ProjectManagement -n promgr-web --hostname www.promanageronline.com
   az containerapp hostname bind -g ProjectManagement -n promgr-web `
     --hostname www.promanageronline.com --environment promgr-cae --validation-method CNAME

   az containerapp hostname add  -g ProjectManagement -n promgr-web --hostname promanageronline.com
   az containerapp hostname bind -g ProjectManagement -n promgr-web `
     --hostname promanageronline.com --environment promgr-cae --validation-method TXT
   ```

The site's nav already includes a **Customer Portal** link to `https://app.promanageronline.com`, so
customers can reach your product app from here. (The **Sign in** link goes to this site's own admin.)

---

## Tuning

Pass these to `deploy.ps1` via `--parameters` overrides (or edit the defaults in `main.bicep`):

- **`minReplicas`** (default `0`): scale-to-zero is cheapest but the first request after idle is slow
  (cold start). Set to `1` to keep one instance always warm.

## Local development is unaffected

`docker compose up --build` still runs the app against its own SQL Server container exactly as before.
This Azure path is entirely separate.

## Validating the template locally

```powershell
az bicep build --file infra/main.bicep --stdout
```
