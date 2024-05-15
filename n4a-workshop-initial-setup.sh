#!/bin/sh

RESOURCE_GROUP_NAME=${RESOURCE_GROUP_NAME:-MyGarage}
OWNER=$(whoami)
LOCATION=westus2
SOURCE_PUBLIC_IP=$(curl -4 ifconfig.co)
GLOBAL_PUBLIC_IP_NAME=$RESOURCE_GROUP_NAME-PIP
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
VM_NAME=$RESOURCE_GROUP_NAME-VM
USERNAME=$(echo "$RESOURCE_GROUP_NAME" | tr '[:upper:]' '[:lower:]')-user
VM_IMAGE="Canonical:0001-com-ubuntu-server-jammy:22_04-lts-gen2:latest"
VNET_NAME=$RESOURCE_GROUP_NAME-VNET
SUBNET_NAME=$RESOURCE_GROUP_NAME-SNET

 ## Create the Resource Group

echo Creating Resource Group...
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION --tags owner=$OWNER

 ## Create the Virtual Network

echo Creating Virtual Network...
az network vnet create \
 --resource-group $RESOURCE_GROUP_NAME \
 --name $VNET_NAME \
 --address-prefixes 172.16.0.0/16

 ## Create the Network Security Group

echo Creating Network Security Group...
az network nsg create --resource-group $RESOURCE_GROUP_NAME --name n4a-nsg

 ## Add two rules to allow HTTP and HTTPS access from the Public IP

 ## Rule 1 for HTTP traffic

echo Creating HTTP Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name n4a-nsg \
--name HTTP \
--priority 320 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 80 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow HTTP traffic"

 ## Rule 2 for HTTPS traffic

echo Creating HTTPS Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name n4a-nsg \
--name HTTPS \
--priority 300 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 443 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow HTTPS traffic"

echo Creating SSH Rule...
az network nsg rule create \
--resource-group $RESOURCE_GROUP_NAME \
--nsg-name n4a-nsg \
--name SSH \
--priority 290 \
--source-address-prefix $SOURCE_PUBLIC_IP \
--source-port-range '*' \
--destination-address-prefix '*' \
--destination-port-range 22 \
--direction Inbound \
--access Allow \
--protocol Tcp \
--description "Allow SSH traffic"


 ## Create subnet for NGINX Azure Resources

echo Creating Subnet for NGINX Azure Resources...

az network vnet subnet create \
--resource-group $RESOURCE_GROUP_NAME \
--name $SUBNET_NAME \
--vnet-name $VNET_NAME \
--address-prefixes 172.16.1.0/24 \
--network-security-group n4a-nsg \
--delegations NGINX.NGINXPLUS/nginxDeployments

 ## Create subnets for AKS (Later lab)

echo Creating Subnets for AKS...
az network vnet subnet create \
--resource-group $RESOURCE_GROUP_NAME \
--name aks1-subnet \
--vnet-name $VNET_NAME \
--address-prefixes 172.16.10.0/23

az network vnet subnet create \
--resource-group $RESOURCE_GROUP_NAME \
--name aks2-subnet \
--vnet-name $VNET_NAME \
--address-prefixes 172.16.20.0/23

 ## Create a Public IP

echo Creating Public IP...
az network public-ip create \
--resource-group $RESOURCE_GROUP_NAME \
--name $GLOBAL_PUBLIC_IP_NAME \
--allocation-method Static \
--sku Standard

GLOBAL_PUBLIC_IP=$(az network public-ip show --resource-group $RESOURCE_GROUP_NAME --name $GLOBAL_PUBLIC_IP_NAME --query ipAddress --output tsv)

 ## Create a User Identity for NGINX Azure Resources. This managed identity will be used to read certificates and keys from Azure keyvault in later labs.

echo Creating User Identity...
az identity create \
--resource-group $RESOURCE_GROUP_NAME \
--name n4a-useridentity

 ## Create a VM to run the My Garage application

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
--public-ip-sku Standard

 ## Retrieve the Public IP of the VM
 VM_PUBLIC_IP=$(az vm show --resource-group $RESOURCE_GROUP_NAME --name $VM_NAME --show-details --query publicIps --output tsv)

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

## Deploy an Nginx for Azure Resource
 
echo Creating NGINX for Azure Resource...
#az nginx deployment create \
#--resource-group $RESOURCE_GROUP_NAME \
#--name nginx4a \
#--location $LOCATION \
#--sku name="standard_Monthly" \
#--network-profile front-end-ip-configuration="{public-ip-addresses:[{id:/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.Network/publicIPAddresses/n4a-publicIP}]}" network-interface-configuration="{subnet-id:/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.Network/virtualNetworks/$VNET_NAME/subnets/$SUBNET_NAME}" \
#--identity="{type:UserAssigned,userAssignedIdentities:{/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP_NAME/providers/Microsoft.ManagedIdentity/userAssignedIdentities/n4a-useridentity:{}}}"


 ## Echo the IPs

echo 
echo "****************************************************************************"
echo "****************************************************************************"
echo
echo The host Public IP is: $SOURCE_PUBLIC_IP
echo The Azure NGINX Public IP is: $GLOBAL_PUBLIC_IP
echo The VMs Public IP is: $VM_PUBLIC_IP
echo
echo set RESOURCE_GROUP_NAME $RESOURCE_GROUP_NAME
echo ./establish-azure-resources.sh
echo
echo "****************************************************************************"
echo "****************************************************************************"
echo 

