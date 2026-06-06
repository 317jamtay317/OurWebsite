# Deploying ProManager Online to Azure

The site runs on **Azure Container Apps** with an **Azure SQL** serverless database, deployed by the
GitHub Actions workflow at [`.github/workflows/deploy.yml`](../.github/workflows/deploy.yml) using
the Bicep template at [`main.bicep`](main.bicep). Every push to `main` builds the image, pushes it to
Docker Hub, and provisions/updates Azure.

## What gets created (resource group `rg-promanageronline-prod`)

| Resource | Purpose |
|---|---|
| Log Analytics workspace | Container logs |
| Storage account + 2 file shares | Persists uploaded doc media and Data Protection keys across restarts |
| Container Apps environment | Hosts the app; scales to zero when idle |
| Azure SQL (serverless, auto-pause) | The database; the app migrates + seeds it on first boot |
| Container App | The running site, pulling the private Docker Hub image |

Estimated cost at low traffic: **~$15–30/month** (mostly SQL + storage; both the app and the database
idle down to near-zero when unused).

---

## One-time setup

Do these once. The easiest place to run the `az` commands is **[Azure Cloud Shell](https://shell.azure.com)**
in **Bash** mode (avoids Windows quoting issues).

### 1. Bootstrap GitHub → Azure auth (OIDC, no stored passwords)

```bash
# --- adjust these three ---
GITHUB_ORG=317jamtay317
GITHUB_REPO=website            # the repo name on GitHub
APP_NAME=promanageronline-github-deploy

RG=rg-promanageronline-prod
LOCATION=eastus2
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
TENANT_ID=$(az account show --query tenantId -o tsv)

# Resource group (the workflow also ensures this, but the role assignment below needs it first)
az group create -n "$RG" -l "$LOCATION"

# App registration + service principal
APP_ID=$(az ad app create --display-name "$APP_NAME" --query appId -o tsv)
az ad sp create --id "$APP_ID"

# Federated credential: trust this repo's main branch (push deploys)
az ad app federated-credential create --id "$APP_ID" --parameters "{
  \"name\": \"github-main\",
  \"issuer\": \"https://token.actions.githubusercontent.com\",
  \"subject\": \"repo:${GITHUB_ORG}/${GITHUB_REPO}:ref:refs/heads/main\",
  \"audiences\": [\"api://AzureADTokenExchange\"]
}"

# Let it manage only this resource group
az role assignment create --assignee "$APP_ID" --role Contributor \
  --scope "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RG}"

# Values to copy into GitHub secrets:
echo "AZURE_CLIENT_ID       = $APP_ID"
echo "AZURE_TENANT_ID       = $TENANT_ID"
echo "AZURE_SUBSCRIPTION_ID = $SUBSCRIPTION_ID"
```

> Manual `workflow_dispatch` runs from a branch are also covered by the `main` credential. If you
> ever deploy from a different branch, add another federated credential for it.

### 2. Create a Docker Hub access token

hub.docker.com → **Account Settings → Personal access tokens → Generate new token**, scope **Read & Write**.
Make sure the repo `jamtay317/promanageronline_website` is **private**.

### 3. Generate a SQL admin password

Any strong password **without spaces** (the value is passed on the deploy command line). For example:

```bash
openssl rand -base64 24 | tr -d '/+=' | cut -c1-24
```

### 4. Add GitHub repository secrets

Repo → **Settings → Secrets and variables → Actions → New repository secret**:

| Secret | Value | Required |
|---|---|---|
| `AZURE_CLIENT_ID` | from step 1 | ✅ |
| `AZURE_TENANT_ID` | from step 1 | ✅ |
| `AZURE_SUBSCRIPTION_ID` | from step 1 | ✅ |
| `DOCKERHUB_USERNAME` | `jamtay317` | ✅ |
| `DOCKERHUB_TOKEN` | from step 2 | ✅ |
| `SQL_ADMIN_PASSWORD` | from step 3 | ✅ |
| `ADMIN_EMAIL` | owner admin login email | ✅ |
| `ADMIN_INITIAL_PASSWORD` | owner admin initial password (no spaces) | ✅ |
| `SMTP_HOST`, `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM_ADDRESS` | contact-form email | optional |
| `RECAPTCHA_SITE_KEY`, `RECAPTCHA_SECRET_KEY` | contact-form reCAPTCHA | optional |

If the SMTP/reCAPTCHA secrets are left unset, the contact form falls back to logging enquiries (no
email) and reCAPTCHA is disabled — fine to start, fill in later.

### 5. First deploy

Push to `main`, or run the workflow manually (**Actions → Deploy to Azure → Run workflow**). The run
prints the live URL (e.g. `https://promgr-web.<region>.azurecontainerapps.io`) in its summary. On the
first boot the app applies migrations, seeds the catalogue, and creates the admin account.

---

## Custom domain (`promanageronline.com`)

Today the apex redirects to `app.promanageronline.com`. To point it at this marketing site instead
(leaving `app.promanageronline.com` — your product app — untouched):

1. **Remove the apex → app redirect** at your domain registrar/DNS.
2. Get the values you'll need from the deployment outputs:
   ```bash
   az containerapp show -g rg-promanageronline-prod -n promgr-web \
     --query "{fqdn:properties.configuration.ingress.fqdn, verify:properties.customDomainVerificationId}" -o table
   az containerapp env show -g rg-promanageronline-prod -n promgr-cae \
     --query properties.staticIp -o tsv
   ```
3. Add DNS records at your registrar:

   | Host | Type | Value |
   |---|---|---|
   | `www` | CNAME | the Container App `fqdn` |
   | `asuid.www` | TXT | the `customDomainVerificationId` |
   | `@` (apex) | A | the environment `staticIp` |
   | `asuid` | TXT | the `customDomainVerificationId` |

4. Bind the hostnames with a free managed certificate (run per hostname):
   ```bash
   az containerapp hostname add  -g rg-promanageronline-prod -n promgr-web --hostname www.promanageronline.com
   az containerapp hostname bind -g rg-promanageronline-prod -n promgr-web \
     --hostname www.promanageronline.com --environment promgr-cae --validation-method CNAME

   az containerapp hostname add  -g rg-promanageronline-prod -n promgr-web --hostname promanageronline.com
   az containerapp hostname bind -g rg-promanageronline-prod -n promgr-web \
     --hostname promanageronline.com --environment promgr-cae --validation-method TXT
   ```

The site's nav already includes a **Customer Portal** link to `https://app.promanageronline.com`, so
customers can reach your product app from here. (The **Sign in** link goes to this site's own admin.)

---

## Tuning

Override these by editing the `parameters:` in the workflow (or pass them to a manual `az deployment`):

- **`minReplicas`** (default `0`): scale-to-zero is cheapest but the first request after idle is slow
  (cold start). Set to `1` to keep one instance always warm.
- **`sqlAutoPauseDelayMinutes`** (default `60`): the serverless DB pauses after this idle period;
  the first query after a pause takes ~30–60s to resume. Set to `-1` to disable auto-pause.

## Local development is unaffected

`docker compose up --build` still runs the app against its own SQL Server container exactly as before.
This Azure path is entirely separate.

## Validating the template locally

```bash
az bicep build --file infra/main.bicep --stdout
```
