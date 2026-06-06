// ==========================================================================
// ProManager Online — Azure infrastructure (resource-group scope).
//
// Provisions everything the site needs to run on Azure Container Apps:
//   • Log Analytics workspace        (container logs)
//   • Storage account + two file shares (uploaded doc media + Data Protection keys)
//   • Container Apps environment     (+ the two file shares wired in as storage)
//   • Azure SQL serverless database  (auto-pauses when idle)
//   • The Container App itself        (pulls the image from Docker Hub)
//
// Secrets are passed in as secure parameters by the GitHub Actions workflow (sourced from GitHub
// repository secrets) and stored as Container App secrets — none are committed to the repo.
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
param dockerHubUsername string

@description('Docker Hub access token (Read) used to pull the private image.')
@secure()
param dockerHubToken string

@description('Azure SQL administrator login name.')
param sqlAdminLogin string = 'pmoadmin'

@description('Azure SQL administrator password. Avoid spaces.')
@secure()
param sqlAdminPassword string

@description('Owner admin email address, seeded on first boot.')
param adminEmail string

@description('Owner admin initial password, seeded on first boot. Avoid spaces.')
@secure()
param adminInitialPassword string

@description('SMTP host for the contact form. Leave blank to use the logging email sender.')
param smtpHost string = ''
param smtpPort int = 587
param smtpUseSsl bool = true
param smtpUser string = ''
@secure()
param smtpPassword string = ''
param smtpFromAddress string = ''
param smtpFromName string = 'ProManager Online'

@description('Google reCAPTCHA site key. Leave blank to disable reCAPTCHA on the contact form.')
param recaptchaSiteKey string = ''
@secure()
param recaptchaSecretKey string = ''

@description('Minimum container replicas. 0 = scale to zero (cheapest, but cold starts). 1 = always warm.')
@minValue(0)
@maxValue(5)
param minReplicas int = 0

@description('Maximum container replicas.')
@minValue(1)
@maxValue(10)
param maxReplicas int = 1

@description('SQL serverless auto-pause delay in minutes. Set to -1 to disable auto-pause (no cold starts, higher cost).')
param sqlAutoPauseDelayMinutes int = 60

@description('Port the container listens on.')
param containerPort int = 8080

// ---- Resource names ----
var suffix = uniqueString(resourceGroup().id)
var logAnalyticsName = '${namePrefix}-logs'
var environmentName = '${namePrefix}-cae'
var containerAppName = '${namePrefix}-web'
var storageAccountName = toLower('st${namePrefix}${substring(suffix, 0, 8)}')
var sqlServerName = toLower('${namePrefix}-sql-${suffix}')
var databaseName = 'ProManagerOnlineSite'
var mediaShareName = 'docs-media'
var keysShareName = 'dataprotection'
var mediaStorageName = 'media'
var keysStorageName = 'keys'

// The app reads its connection string from this Container App secret. Built from the SQL server FQDN
// and the admin credentials; a generous timeout covers the serverless database resuming from pause.
var connectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${databaseName};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=true;Connection Timeout=60;'

// ---- Container App secrets (optional ones are added only when supplied) ----
var baseSecrets = [
  { name: 'dockerhub-token', value: dockerHubToken }
  { name: 'connection-string', value: connectionString }
  { name: 'admin-password', value: adminInitialPassword }
]
var smtpSecret = (empty(smtpHost) || empty(smtpPassword)) ? [] : [ { name: 'smtp-password', value: smtpPassword } ]
var recaptchaSecret = (empty(recaptchaSiteKey) || empty(recaptchaSecretKey)) ? [] : [ { name: 'recaptcha-secret', value: recaptchaSecretKey } ]
var secrets = concat(baseSecrets, smtpSecret, recaptchaSecret)

// ---- Container environment variables (mirror appsettings.json section names) ----
var baseEnv = [
  { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
  { name: 'ASPNETCORE_HTTP_PORTS', value: string(containerPort) }
  { name: 'ConnectionStrings__SiteDatabase', secretRef: 'connection-string' }
  { name: 'Admin__Email', value: adminEmail }
  { name: 'Admin__InitialPassword', secretRef: 'admin-password' }
  { name: 'DataProtection__KeyRingPath', value: '/keys' }
]
var smtpEnv = empty(smtpHost) ? [] : [
  { name: 'Smtp__Host', value: smtpHost }
  { name: 'Smtp__Port', value: string(smtpPort) }
  { name: 'Smtp__UseSsl', value: toLower(string(smtpUseSsl)) }
  { name: 'Smtp__User', value: smtpUser }
  { name: 'Smtp__FromAddress', value: smtpFromAddress }
  { name: 'Smtp__FromName', value: smtpFromName }
]
var smtpPwdEnv = (empty(smtpHost) || empty(smtpPassword)) ? [] : [ { name: 'Smtp__Password', secretRef: 'smtp-password' } ]
var recaptchaEnv = empty(recaptchaSiteKey) ? [] : [ { name: 'Recaptcha__SiteKey', value: recaptchaSiteKey } ]
var recaptchaSecretEnv = (empty(recaptchaSiteKey) || empty(recaptchaSecretKey)) ? [] : [ { name: 'Recaptcha__SecretKey', secretRef: 'recaptcha-secret' } ]
var containerEnv = concat(baseEnv, smtpEnv, smtpPwdEnv, recaptchaEnv, recaptchaSecretEnv)

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

// ---- Azure SQL (serverless, auto-pause) ----
resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: sqlAutoPauseDelayMinutes
    minCapacity: json('0.5')
    maxSizeBytes: 2147483648
    zoneRedundant: false
  }
}

// Lets the Container App (an Azure service with dynamic egress IPs) reach the SQL server.
resource sqlFirewallAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAllAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ---- Container App ----
resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
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
  dependsOn: [ mediaEnvStorage, keysEnvStorage, sqlFirewallAzure ]
}

// ---- Outputs (used by the workflow and the DNS / custom-domain step) ----
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output environmentStaticIp string = environment.properties.staticIp
output customDomainVerificationId string = containerApp.properties.customDomainVerificationId
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
