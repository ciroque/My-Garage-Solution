$Ticks = (Get-Date).Ticks
$Location = "westus2"
$ResourceGroupName = "MyGarageRG"
$StorageAccountName = "mygsa" + $Ticks
$StorageContainerName = "mygsc" + $Ticks
$SasTokenName = "mygsas" + $Ticks
$SasExpiry = "2024-05-31T23:59:59Z"
$AppConfigName = "mygac" + $Ticks
$SasTokenAppConfigKey = "AzureStorageSasToken"
$StorageConnectionStringConfigKey = "AzureStorageConnectionString"
$StorageContainerNameConfigKey = "AzureStorageContainerName"
$RedisConnectionStringConfigKey = "RedisConnectionString"
$owner = $(whoami)

clear

<# -------------------------------- #>
<# Clean Up Previous Run #>

echo "Deleting existing resources..."

az group delete --name $ResourceGroupName --yes

<# -------------------------------- #>
<# Create a new set of stuff #>

echo "Creating resources..."

#-- Resource Group, The Root of All Things
az group create --name $ResourceGroupName --location $Location --tags owner=$owner

#-- Storage Account, The Place to Store Stuff
# * The --allow-blob-public-access true is required to allow the container to be public
az storage account create --name $StorageAccountName --resource-group $ResourceGroupName --location $Location --sku Standard_LRS --kind StorageV2 --access-tier Cool --allow-blob-public-access true --tags environment=production owner=$owner

#-- The Account Key is needed for subsequent resources
$accountKey=$(az storage account keys list --resource-group $ResourceGroupName --account-name $StorageAccountName --query "[0].value" --output tsv)

#-- CORS ensure that the storage account can be accessed from the web
az storage cors add --account-name $StorageAccountName --account-key $accountKey --services b --origins "*" --methods GET HEAD --allowed-headers "*" --exposed-headers "*" --max-age 3600

#-- The Storage Connection String is necessary for the App Configuration Store, it is used to let the Application store images
$storageConnectionString=$(az storage account show-connection-string --name $StorageAccountName --resource-group $ResourceGroupName --output tsv)

#-- Storage Container, The Place to Store Images
az storage container create --name $StorageContainerName --account-name $StorageAccountName --account-key $accountKey --public-access blob

#-- Is this really necessary??
#az storage container set-permission --name $StorageContainerName --account-name $StorageAccountName --account-key $accountKey --public-access blob

#-- The App Configuration Store, The Place to Store Configuration
az appconfig create --name $AppConfigName --resource-group $ResourceGroupName --location $Location --sku Standard --query id --output tsv

#-- The Web Application -- My Garage -- needs this to be able to connect to the AppConfig instance and grab configuration
$appConfigConnectionString=$(az appconfig credential list --name $AppConfigName --resource-group $ResourceGroupName --query "[?name=='Primary Read Only'].connectionString" -o tsv)

#-- The values required by the application need to be seeded. Note that all except for the RedisConnectionString have been gathered by this script
az appconfig kv set --yes --name $AppConfigName --key $StorageConnectionStringConfigKey --value "$storageConnectionString"
az appconfig kv set --yes --name $AppConfigName --key $StorageContainerNameConfigKey --value "$StorageContainerName"
az appconfig kv set --yes --name $AppConfigName --key $RedisConnectionStringConfigKey --value "kungdu.wagner-x.net"

#echo "Resource Group ID: $(az group show --name $ResourceGroupName --query id --output tsv)"
#echo "Storage Account ID: $(az storage account show --name $StorageAccountName --resource-group $ResourceGroupName --query id --output tsv)"
#echo "App Configuration Store ID: $(az appconfig show --name $AppConfigName --resource-group $ResourceGroupName --query id --output tsv)"
#echo "Storage Container ID: $(az storage container show --name $StorageContainerName --account-name $StorageAccountName --account-key $accountKey --query id --output tsv)"
#echo "Account Key: $accountKey"
#echo "Storage Connection String: $storageConnectionString"
#echo "Storage Container Name: $StorageContainerName"

#-- Burp out the AppConfig ConnectionString so it can be included in the MyGarage application startup. It is an argument to the docker-compose command:
# docker-compose -e AppConfigurationConnectionString=$appConfigConnectionString up 
echo "AppConfig Connection String: $appConfigConnectionString"
