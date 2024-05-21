#!/bin/sh

# establish-azure-resources.sh
#
# This script creates all the Micorosft Azure Resources needed to run the My Garage application.
#
# The script assumes that you have the Azure CLI installed and configured to point to the desired Azure Subscription.

function calculate_ticks() {
  timestamp=$(date +%s)
  milliseconds=$(date +%N | awk '{print $1 / 1000}')
  ticks="$timestamp$milliseconds"
  echo $ticks | sed 's/\.//g'
}

clear

# These need to be set by the user before running the script
#resourceGroupName="MyGarageRG"
resourceGroupName=${RESOURCE_GROUP_NAME=MyGarage}
sasExpiry="2024-12-31T23:59:59Z"
redisConnectionString="redis.example.com"
theGarageUrl="http://the-garage.example.com"

ticks=$(calculate_ticks)
location=westus2
storageAccountName="mygsa$ticks"
storageContainerName="mygsc$ticks"
sasTokenName="mygsas$ticks"
appConfigName="mygac$ticks"
owner=$(whoami)

# These correspond to the keys in the AppConfig that the My Garage application will use to access the resources, do not modify
storageConnectionStringConfigKey="AzureStorageConnectionString"      
storageContainerNameConfigKey="AzureStorageContainerName"
redisConnectionStringConfigKey="RedisConnectionString"
theGarageUrlConfigKey="TheGarageUrl"


## Create the stuff
echo Creating Resources in Resource Group ""$resourceGroupName""...

# Storage Account, The Place to Store Stuff
# * The --allow-blob-public-access true is required to allow the container to be public
az storage account create --name $storageAccountName --resource-group $resourceGroupName --location $location --sku Standard_LRS --kind StorageV2 --access-tier Cool --allow-blob-public-access true --tags owner=$owner

# The Account Key is needed for subsequent resources
accountKey=$(az storage account keys list --resource-group $resourceGroupName --account-name $storageAccountName --query "[0].value" --output tsv)

# CORS ensure that the storage account can be accessed from the web
az storage cors add --account-name $storageAccountName --account-key $accountKey --services b --origins "*" --methods GET HEAD --allowed-headers "*" --exposed-headers "*" --max-age 3600

# The Storage Connection String is necessary for the App Configuration Store, it is used to let the Application store images
storageConnectionString=$(az storage account show-connection-string --name $storageAccountName --resource-group $resourceGroupName --output tsv)

# Storage Container, The Place to Store Images
az storage container create --name $storageContainerName --account-name $storageAccountName --account-key $accountKey --public-access blob

# The App Configuration Store, The Place to Store Configuration
az appconfig create --name $appConfigName --resource-group $resourceGroupName --location $location --sku Standard --query id --output tsv

# The Web Application -- My Garage -- needs this to be able to connect to the AppConfig instance and grab configuration
appConfigConnectionString=$(az appconfig credential list --name $appConfigName --resource-group $resourceGroupName --query "[?name=='Primary Read Only'].connectionString" -o tsv)

# The values required by the application need to be seeded. Note that all except for the RedisConnectionString have been gathered by this script
az appconfig kv set --yes --name $appConfigName --key $storageConnectionStringConfigKey --value "$storageConnectionString"
az appconfig kv set --yes --name $appConfigName --key $storageContainerNameConfigKey --value "$storageContainerName"
az appconfig kv set --yes --name $appConfigName --key $redisConnectionStringConfigKey --value "$redisConnectionString"
az appconfig kv set --yes --name $appConfigName --key $theGarageConfigKey --value "$theGarageUrl"

# Burp out the AppConfig ConnectionString so it can be included in the MyGarage application startup. It is an argument to the docker-compose command:
# The docker-compose file will be updated to include this value in the environment variables
echo "AppConfig Connection String: $appConfigConnectionString"

sed -i "5s/$/$appConfigConnectionString" docker-compose.yaml
