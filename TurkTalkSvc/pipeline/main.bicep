@description('Environment short code')
param environment_code string = 'dev'

@description('The prefix for Azure resources (e.g. kuvadev).')
param resource_prefix string = 'olab'

// Note: Array of allowable values not recommended by Microsoft in this case as the list of SKUs can be different per region
@description('Describes plan\'s pricing tier and capacity. Check details at https://azure.microsoft.com/en-us/pricing/details/app-service/')
param sku string = 'B1'

@description('The location to deploy the service resources')
param location string = 'canadacentral'

@description('MySQL database user id')
param MySqlUserId string = 'olab4admin'

@description('MySQL database user id')
param MySqlDatabaseId string = 'olab45'

@description('MySQL database host name')
param MySqlHostName string = 'olab45db'

@description('MySQL database password')
@secure()
param MySqlPassword string

@description('Auth Token key')
@secure()
param AuthTokenKey string

// Variables
// Note: Declaring variable blocks is not recommended by Microsoft
var serviceName = 'signalrapi'
var resourceNameFunctionApp = '${resource_prefix}${environment_code}${serviceName}'
var resourceNameFunctionAppFarm = resourceNameFunctionApp
var resourceNameFunctionAppInsights = resourceNameFunctionApp
var resourceNameFunctionAppStorage = '${resource_prefix}${environment_code}apiaz'
var resourceNameFileStorage = '${resource_prefix}${environment_code}api'
var resourceNameSignalr = '${resource_prefix}signalr'

resource appService 'Microsoft.Web/sites@2021-02-01' = {
  name: resourceNameFunctionApp
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    disposable: 'yes'
  }
  properties: {
    serverFarmId: appServicePlan.id
    enabled: true
    httpsOnly: true
    hostNamesDisabled: false
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|7.0'
      alwaysOn: true
      healthCheckPath: '/api/health'
      use32BitWorkerProcess : false
    }
  }
}

resource appSettings 'Microsoft.Web/sites/config@2021-02-01' = {
  name: 'appsettings'
  parent: appService
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
    AppSettings__Audience: 'https://www.olab.ca'
    AppSettings__FileStorageConnectionString: fileStorageConnectionString
    AppSettings__Issuer: 'olab,moodle'
    AppSettings__Secret: AuthTokenKey
    AppSettings__SignalREndpoint: '/turktalk'
    AppSettings__TokenExpiryMinutes: '360'
    AzureWebJobsStorage: functionAppStorageConnectionString
    DefaultDatabase: 'server=${MySqlHostName}.mysql.database.azure.com;uid=${MySqlUserId};pwd=${MySqlPassword};database=${MySqlDatabaseId};ConvertZeroDateTime=True'
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: functionAppStorageConnectionString
    WEBSITE_CONTENTSHARE: '${resourceNameFunctionApp}9552'
    WEBSITE_RUN_FROM_PACKAGE: '1'
  }
}

resource appCors 'Microsoft.Web/sites/config@2021-02-01' = {
  parent: appService
  name: 'web'
  properties: {
    cors: {
      allowedOrigins: [
        '*'
        'https://portal.azure.com'
        'http://localhost:3000'
      ]
      supportCredentials: false
    }
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: resourceNameFunctionAppFarm
  location: location
  kind: 'linux'
  sku: {
    name: sku
  }
  tags: {
    disposable: 'yes'
  }
  properties: {
    // Note: These properties probably not required
    perSiteScaling: false
    elasticScaleEnabled: false
    maximumElasticWorkerCount: 1
    isSpot: false
    reserved: true
    isXenon: false
    hyperV: false
    targetWorkerCount: 0
    targetWorkerSizeId: 0
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceNameFunctionAppInsights
  location: location
  kind: 'web'
  tags: {
    disposable: 'yes'
  }
  properties: {
    Application_Type: 'web'

    // Note: These properties probably not required
    RetentionInDays: 90
    IngestionMode: 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

var functionAppStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceFunctionAppStorage.name};AccountKey=${resourceFunctionAppStorage.listkeys().keys[0].value};EndpointSuffix=core.windows.net'
resource resourceFunctionAppStorage 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: resourceNameFunctionAppStorage
}

var fileStorageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${resourceFileStorage.name};AccountKey=${resourceFileStorage.listkeys().keys[0].value};EndpointSuffix=core.windows.net'
resource resourceFileStorage 'Microsoft.Storage/storageAccounts@2021-06-01' existing = {
  name: resourceNameFileStorage
}
