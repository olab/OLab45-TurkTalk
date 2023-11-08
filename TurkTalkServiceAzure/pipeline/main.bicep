@description('Environment short code')
param environment_code string = 'dev'

@description('The prefix for Azure resources (e.g. kuvadev).')
param resource_prefix string = 'olab'

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
var serviceName = 'api'
var resourceNameFunctionApp = '${resource_prefix}${environment_code}${serviceName}'
var resourceNameFunctionAppFarm = resourceNameFunctionApp
var resourceNameFunctionAppInsights = resourceNameFunctionApp
var resourceNameFunctionAppStorage = '${resourceNameFunctionApp}az'
var resourceNameFileStorage = resourceNameFunctionApp
var resourceNamePlayer = '${resource_prefix}${environment_code}player'
var resourceNameSignalr = '${resource_prefix}signalr'
var resourceNameCosmosDb = '${resource_prefix}db'
var fileStorageContainerName = '$web'
var fileStorageFilesFolder = 'files'
var fileStorageImportFolder = 'import'

var signalrConnectionString = resourceSignalr.listKeys().primaryConnectionString
resource resourceSignalr 'Microsoft.SignalRService/SignalR@2022-02-01' = {
  name: resourceNameSignalr
  location: location
  sku: {
    name: 'Free_F1'
    tier: 'Free'
    capacity: 1
  }
  tags: {
    disposable: 'yes'
  }
  kind: 'SignalR'
  properties: {
    tls: {
      clientCertEnabled: false
    }
    features: [
      {
        flag: 'ServiceMode'
        value: 'Serverless'
      }
      {
        flag: 'EnableConnectivityLogs'
        value: 'True'
      }
    ]
    liveTraceConfiguration: {
      enabled: 'true'
      categories: [
        {
          name: 'ConnectivityLogs'
          enabled: 'true'
        }
        {
          name: 'MessagingLogs'
          enabled: 'true'
        }
        {
          name: 'HttpRequestLogs'
          enabled: 'true'
        }
      ]
    }
    cors: {
      allowedOrigins: [
        '*'
      ]
    }
    upstream: {
      templates: []
    }
    networkACLs: {
      defaultAction: 'Deny'
      publicNetwork: {
        allow: [
          'ServerConnection'
          'ClientConnection'
          'RESTAPI'
          'Trace'
        ]
      }
      privateEndpoints: []
    }
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    disableAadAuth: false
  }

}
