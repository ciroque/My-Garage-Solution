#!/bin/sh

## ----------------------------------------------------------------------------
## Establish names and configuration for the Azure Resources
## 
## These values are used when creating the various Resourses needed to run the application
## ----------------------------------------------------------------------------

RESOURCE_GROUP_NAME=${RESOURCE_GROUP_NAME:-MyGarage}
RESOURCE_GROUP_NAME_LC=$(echo "$RESOURCE_GROUP_NAME" | tr '[:upper:]' '[:lower:]')
OWNER=$(whoami)
LOCATION=westus2
SOURCE_PUBLIC_IP=$(curl -4 ifconfig.co)
GLOBAL_PUBLIC_IP_NAME=$RESOURCE_GROUP_NAME-PIP
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
NSG_NAME=$RESOURCE_GROUP_NAME_LC-nsg
USER_IDENTITY_NAME=$RESOURCE_GROUP_NAME_LC-userid
VM_NAME=$RESOURCE_GROUP_NAME-VM
USERNAME=$RESOURCE_GROUP_NAME_LC-user
VM_IMAGE="Canonical:0001-com-ubuntu-server-jammy:22_04-lts-gen2:latest"
VNET_NAME=$RESOURCE_GROUP_NAME_LC-vnet
SUBNET_NAME=$RESOURCE_GROUP_NAME_LC-snet
SAS_EXPIRY="2024-12-31T23:59:59Z"
REDIS_CONNECTION_STRING="redis.example.com"

TICKS=$(calculate_ticks)
STORAGE_ACCOUNT_NAME="mygsa$ticks"
STORAGE_CONTAINER_NAME="mygsc$ticks"
SAS_TOKEN_NAME="mygsas$ticks"
APP_CONFIG_NAME="mygac$ticks"

## ----------------------------------------------------------------------------
## These correspond to the keys in the AppConfig that the My Garage application will use to access the resources, do not modify
## ----------------------------------------------------------------------------

SAS_TOKEN_APPCONFIG_KEY="AzureStorageSasToken"
STORAGE_CONNECTION_STRING_CONFIG_KEY="AzureStorageConnectionString"      
STORAGE_CONTAINER_NAME_CONFIG_KEY="AzureStorageContainerName"
REDIS_CONNECTION_STRING_CONFIG_KEY="RedisConnectionString"

## ----------------------------------------------------------------------------
## Create the Resource Group
##
## The Resource Group is the primary container for all the Resources.
## ----------------------------------------------------------------------------

echo Creating Resource Group...
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION --tags owner=$OWNER --quiet

## ----------------------------------------------------------------------------
## Create the Virtual Network
## 
## The Virtual Network is the network that the Virtual Machine will be connected to.
## ----------------------------------------------------------------------------

echo Creating Virtual Network...
az network vnet create \
 --resource-group $RESOURCE_GROUP_NAME \
 --name $VNET_NAME \
 --address-prefixes 172.16.0.0/16 \
 --quiet

## ----------------------------------------------------------------------------
## Create the Network Security Group
##
## The Security Group allows controlling access to the Resources.
## ----------------------------------------------------------------------------

echo Creating Network Security Group...
az network nsg create --resource-group $RESOURCE_GROUP_NAME --name $NSG_NAME --quiet

## ----------------------------------------------------------------------------
## Add rules to allow HTTP, HTTPS, and SSH access from the Public IP of the current host
## ----------------------------------------------------------------------------

## Rule 1 for HTTP traffic

echo Creating HTTP Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name $NSG_NAME \
--name HTTP \
--priority 320 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 80 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow HTTP traffic" \
--quiet

 ## Rule 2 for HTTPS traffic

echo Creating HTTPS Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name $NSG_NAME \
--name HTTPS \
--priority 300 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 443 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow HTTPS traffic" \
--quiet

echo Creating SSH Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name $NSG_NAME \
--name SSH \
--priority 290 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 22 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow SSH traffic" \
--quiet

## ----------------------------------------------------------------------------
## Create subnet for NGINX Azure Resources
##
## The Subnet is the network that the NGINX Resources will be connected to.
## ----------------------------------------------------------------------------

echo Creating Subnet for NGINX Azure Resources...

az network vnet subnet create \
--resource-group $RESOURCE_GROUP_NAME \
--name $SUBNET_NAME \
--vnet-name $VNET_NAME \
--address-prefixes 172.16.1.0/24 \
--network-security-group n4a-nsg \
--delegations NGINX.NGINXPLUS/nginxDeployments \
--quiet

## ----------------------------------------------------------------------------
## Create a Public IP
##
## The Public IP is the IP address for the Virtual Machine.
## ----------------------------------------------------------------------------

echo Creating Public IP...
az network public-ip create \
--resource-group $RESOURCE_GROUP_NAME \
--name $GLOBAL_PUBLIC_IP_NAME \
--allocation-method Static \
--sku Standard \
--quiet

## Stash the Public IP for later...
GLOBAL_PUBLIC_IP=$(az network public-ip show --resource-group $RESOURCE_GROUP_NAME --name $GLOBAL_PUBLIC_IP_NAME --query ipAddress --output tsv)

## ----------------------------------------------------------------------------
## Create a User Identity for NGINX Azure Resources. This managed identity will be used to read certificates and keys from Azure keyvault in later labs.
## ----------------------------------------------------------------------------

echo Creating User Identity...
az identity create \
--resource-group $RESOURCE_GROUP_NAME \
--name $USER_IDENTITY_NAME \
--quiet

## ----------------------------------------------------------------------------
## Create a VM to run the My Garage application
## ----------------------------------------------------------------------------

echo Creating VM...
az vm create \
--resource-group $RESOURCE_GROUP_NAME \
--location $LOCATION \
--tags owner=$OWNER \
--name $VM_NAME \
--image $VM_IMAGE \
--admin-username $USERNAME \
--vnet-name $VNET_NAME \
--subnet $SUBNET_NAME \
--assign-identity \
--generate-ssh-keys \
--public-ip-sku Standard \
--quiet

 ## Stash the Public IP for later...
 VM_PUBLIC_IP=$(az vm show --resource-group $RESOURCE_GROUP_NAME --name $VM_NAME --show-details --query publicIps --output tsv)

## ----------------------------------------------------------------------------
## Install Docker and docker-compose on the VM

echo Installing Docker and docker-compose on the VM...
cat << EOF > install_docker_on_vm.sh 
#!/bin/sh

ssh "${USERNAME}@${VM_PUBLIC_IP}" '{
  {
    ARCH='"$(dpkg --print-architecture)"'
    echo The Architecture is: $ARCH
    sudo apt update &&
    sudo apt install -y apt-transport-https ca-certificates curl software-properties-common &&
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg &&
    echo "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu jammy stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null &&
    cat /etc/apt/sources.list.d/docker.list &&
    sudo apt update &&
    apt-cache policy docker-ce &&
    sudo apt install -y docker-ce 
  } &&
  {
    sudo apt install -y docker-compose &&
    mkdir -p ~/.docker/cli-plugins/ &&
    curl -SL https://github.com/docker/compose/releases/download/v2.3.3/docker-compose-linux-x86_64 -o ~/.docker/cli-plugins/docker-compose &&
    chmod +x ~/.docker/cli-plugins/docker-compose    
  }
}'

EOF

chmod +x install_docker_on_vm.sh

echo Running remote script on VM...
./install_docker_on_vm.sh

## ----------------------------------------------------------------------------
## Deploy an Nginx for Azure Resource
## ----------------------------------------------------------------------------

##! Commented out for now as this takes a while... 

echo Creating NGINX for Azure Resource...
#az nginx deployment create \
#--resource-group $RESOURCE_GROUP_NAME \
#--name nginx4a \
#--location $LOCATION \
#--sku name="standard_Monthly" \
#--network-profile front-end-ip-configuration="{public-ip-addresses:[{id:/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.Network/publicIPAddresses/n4a-publicIP}]}" network-interface-configuration="{subnet-id:/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.Network/virtualNetworks/$VNET_NAME/subnets/$SUBNET_NAME}" \
#--identity="{type:UserAssigned,userAssignedIdentities:{/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.ManagedIdentity/userAssignedIdentities/n4a-useridentity:{}}}"

## ----------------------------------------------------------------------------
# Storage Account, The Place to Store Stuff
# * The --allow-blob-public-access true is required to allow the container to be public
## ----------------------------------------------------------------------------

az storage account create \
--name $STORAGE_ACCOUNT_NAME \
--resource-group $RESOURCE_GROUP_NAME \
--location $LOCATION \
--sku Standard_LRS \
--kind StorageV2 \
--access-tier Cool \
--allow-blob-public-access true 
--tags owner=$OWNER \
--quiet

## Stash the Account Key for later...
ACCOUNT_KEY=$(az storage account keys list --resource-group $resourceGroupName --account-name $storageAccountName --query "[0].value" --output tsv)

## CORS ensure that the storage account can be accessed from the web
az storage cors add \
--account-name $STORAGE_ACCOUNT_NAME \
--account-key $ACCOUNT_KEY \
--services b \
--origins "*" \
--methods GET HEAD \
--allowed-headers "*" \
--exposed-headers "*" \
--max-age 3600 \
--quiet

# The Storage Connection String is necessary for the App Configuration Store, it is used to let the Application store images
STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name $storageAccountName --resource-group $resourceGroupName --output tsv)

## ----------------------------------------------------------------------------
## Storage Container, The Place to Store Images
## ----------------------------------------------------------------------------

az storage container create \
--name $STORAGE_CONTAINER_NAME \
--account-name $STORAGE_ACCOUNT_NAME \
--account-key $ACCOUNT_KEY \
--public-access blob \
--quiet

## ----------------------------------------------------------------------------
## The App Configuration Store, The Place to Store Configuration
## ----------------------------------------------------------------------------

az appconfig create \
--name $APP_CONFIG_NAME \
--resource-group $RESOURCE_GROUP_NAME \
--location $LOCATION \
--sku Standard \
--query id \
--output tsv

## Stash the Web Application for later...
APP_CONFIG_CONNECTION_STRING=$(az appconfig credential list --name $APP_CONFIG_NAME --resource-group $RESOURCE_GROUP_NAME --query "[?name=='Primary Read Only'].connectionString" -o tsv)

## ----------------------------------------------------------------------------
## The values required by the application need to be seeded. Note that all except for the RedisConnectionString have been gathered by this script
## ----------------------------------------------------------------------------

az appconfig kv set \
--yes \
--name $APP_CONFIG_NAME \
--key $STORAGE_CONNECTION_STRING_CONFIG_KEY \
--value "$STORAGE_CONNECTION_STRING" \
--quiet

az appconfig kv set \
--yes \
--name $APP_CONFIG_NAME \
--key $STORAGE_CONTAINER_NAME_CONFIG_KEY \
--value "$STORAGE_CONTAINER_NAME" \
--quiet

az appconfig kv set \
--yes \
--name $APP_CONFIG_NAME \
--key $REDIS_CONNECTION_STRING_CONFIG_KEY \
--value "$REDIS_CONNECTION_STRING" \
--quiet

## ----------------------------------------------------------------------------
## Burp out the AppConfig ConnectionString so it can be included in the MyGarage application startup. It is an argument to the docker-compose command:
## The docker-compose file will be updated to include this value in the environment variables
echo "AppConfig Connection String: $appConfigConnectionString"

## ----------------------------------------------------------------------------
## Update the docker-compose.yaml file with the AppConfig Connection String
## ----------------------------------------------------------------------------

sed -i "5s/$/$appConfigConnectionString" docker-compose.yaml

## ----------------------------------------------------------------------------
## TODO: scp the docker-compose.yaml file to the VM
## ----------------------------------------------------------------------------
