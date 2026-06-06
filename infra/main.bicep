// ==========================================================================
// ProManager Online — Azure infrastructure (resource-group scope).
//
// Deploys the site to Azure Container Apps. All runtime secrets live in an existing Azure Key Vault
// and are read by the app at runtime via a user-assigned managed identity — nothing sensitive is
// passed into this template or stored outside Azure.
//
// Provisions:
//   • Log Analytics workspace        (container logs)
//   • Storage account + two file shares (uploaded doc media + Data Protection keys)
//   • Container Apps environment     (+ the two file shares wired in as storage)
//   • User-assigned managed identity (+ Key Vault Secrets User on the vault)
//   • The Container App itself        (pulls the image from Docker Hub)
//
// The database is hosted externally; its connection string is a Key Vault secret.
// Run via infra/deploy.ps1 (which builds + pushes the image, then deploys this).
// ==========================================================================

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Short, alphanumeric prefix used to name resources (lowercase letters and digits only).')
@minLength(3)
@maxLength(11)
param namePrefix string = 'promgr'

@description('Fully qualified container image including tag, e.g. docker.io/jamtay317/promanageronline_website:abc123.')
param containerImage string

@description('Docker Hub username used to pull the private image.')
param dockerHubUsername string = 'jamtay317'

@description('Name of the existing Key Vault (in this resource group) holding the app secrets.')
param keyVaultName string

// Names of the secrets the app expects to find in the Key Vault.
param sqlConnectionSecretName string = 'sql-connection-string'
param adminEmailSecretName string = 'admin-email'
param adminPasswordSecretName string = 'admin-initial-password'
param dockerHubTokenSecretName string = 'dockerhub-token'

@description('Enable SMTP for the contact form. Requires the smtp-password secret in the vault.')
param enableSmtp bool = false
param smtpHost string = ''
param smtpPort int = 587
param smtpUseSsl bool = true
param smtpUser string = ''
param smtpFromAddress string = ''
param smtpFromName string = 'ProManager Online'
param smtpPasswordSecretName string = 'smtp-password'

@description('Enable reCAPTCHA on the contact form. Requires the recaptcha-secret secret in the vault.')
param enableRecaptcha bool = false
param recaptchaSiteKey string = ''
param recaptchaSecretName string = 'recaptcha-secret'

@description('Minimum container replicas. 0 = scale to zero (cheapest, but cold starts). 1 = always warm.')
@minValue(0)
@maxValue(5)
param minReplicas int = 0

@description('Maximum container replicas.')
@minValue(1)
@maxValue(10)
param maxReplicas int = 1

@description('Port the container listens on.')
param containerPort int = 8080

// ---- Resource names ----
var suffix = uniqueString(resourceGroup().id)
var logAnalyticsName = '${namePrefix}-logs'
var environmentName = '${namePrefix}-cae'
var containerAppName = '${namePrefix}-web'
var identityName = '${namePrefix}-id'
var storageAccountName = toLower('st${namePrefix}${substring(suffix, 0, 8)}')
var mediaShareName = 'docs-media'
var keysShareName = 'dataprotection'
var mediaStorageName = 'media'
var keysStorageName = 'keys'

// Built-in role: Key Vault Secrets User (read secret values).
var keyVaultSecretsUserRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')

// ---- Existing Key Vault (created out of band; holds the secrets) ----
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// ---- User-assigned managed identity the Container App uses to read the vault ----
resource identity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: identityName
  location: location
}

resource keyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, identity.id, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: keyVaultSecretsUserRoleId
    principalId: identity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// ---- Container App secrets (Key Vault references; optional ones added only when enabled) ----
var baseSecrets = [
  { name: 'connection-string', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${sqlConnectionSecretName}', identity: identity.id }
  { name: 'admin-email', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${adminEmailSecretName}', identity: identity.id }
  { name: 'admin-password', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${adminPasswordSecretName}', identity: identity.id }
  { name: 'dockerhub-token', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${dockerHubTokenSecretName}', identity: identity.id }
]
var smtpSecret = enableSmtp ? [ { name: 'smtp-password', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${smtpPasswordSecretName}', identity: identity.id } ] : []
var recaptchaSecret = enableRecaptcha ? [ { name: 'recaptcha-secret', keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${recaptchaSecretName}', identity: identity.id } ] : []
var secrets = concat(baseSecrets, smtpSecret, recaptchaSecret)

// ---- Container environment variables (mirror appsettings.json section names) ----
var baseEnv = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'ASPNETCORE_HTTP_PORTS', value: string(containerPort) }
  { name: 'ConnectionStrings__SiteDatabase', secretRef: 'connection-string' }
  { name: 'Admin__Email', secretRef: 'admin-email' }
  { name: 'Admin__InitialPassword', secretRef: 'admin-password' }
  { name: 'DataProtection__KeyRingPath', value: '/keys' }
]
var smtpEnv = enableSmtp ? [
  { name: 'Smtp__Host', value: smtpHost }
  { name: 'Smtp__Port', value: string(smtpPort) }
  { name: 'Smtp__UseSsl', value: toLower(string(smtpUseSsl)) }
  { name: 'Smtp__User', value: smtpUser }
  { name: 'Smtp__Password', secretRef: 'smtp-password' }
  { name: 'Smtp__FromAddress', value: smtpFromAddress }
  { name: 'Smtp__FromName', value: smtpFromName }
] : []
var recaptchaEnv = enableRecaptcha ? [
  { name: 'Recaptcha__SiteKey', value: recaptchaSiteKey }
  { name: 'Recaptcha__SecretKey', secretRef: 'recaptcha-secret' }
] : []
var containerEnv = concat(baseEnv, smtpEnv, recaptchaEnv)

// ---- Log Analytics ----
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

// ---- Storage account + file shares (durable doc media and Data Protection keys) ----
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource mediaShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: fileService
  name: mediaShareName
  properties: { accessTier: 'TransactionOptimized' }
}

resource keysShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-05-01' = {
  parent: fileService
  name: keysShareName
  properties: { accessTier: 'TransactionOptimized' }
}

// ---- Container Apps environment ----
resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource mediaEnvStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: environment
  name: mediaStorageName
  properties: {
    azureFile: {
      accountName: storage.name
      accountKey: storage.listKeys().keys[0].value
      shareName: mediaShareName
      accessMode: 'ReadWrite'
    }
  }
  dependsOn: [ mediaShare ]
}

resource keysEnvStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: environment
  name: keysStorageName
  properties: {
    azureFile: {
      accountName: storage.name
      accountKey: storage.listKeys().keys[0].value
      shareName: keysShareName
      accessMode: 'ReadWrite'
    }
  }
  dependsOn: [ keysShare ]
}

// ---- Container App ----
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'auto'
        allowInsecure: false
        traffic: [ { latestRevision: true, weight: 100 } ]
      }
      registries: [
        { server: 'docker.io', username: dockerHubUsername, passwordSecretRef: 'dockerhub-token' }
      ]
      secrets: secrets
    }
    template: {
      containers: [
        {
          name: 'web'
          image: containerImage
          resources: { cpu: json('0.5'), memory: '1Gi' }
          env: containerEnv
          volumeMounts: [
            { volumeName: 'media', mountPath: '/app/wwwroot/docs-media' }
            { volumeName: 'keys', mountPath: '/keys' }
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
      }
      volumes: [
        { name: 'media', storageType: 'AzureFile', storageName: mediaStorageName }
        { name: 'keys', storageType: 'AzureFile', storageName: keysStorageName }
      ]
    }
  }
  // The managed identity must be able to read the vault before the app resolves its secret references.
  dependsOn: [ keyVaultSecretsUser, mediaEnvStorage, keysEnvStorage ]
}

// ---- Outputs ----
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output environmentStaticIp string = environment.properties.staticIp
output customDomainVerificationId string = containerApp.properties.customDomainVerificationId
output managedIdentityPrincipalId string = identity.properties.principalId
