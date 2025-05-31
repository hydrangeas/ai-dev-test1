# Output values for Azure IoT Hub infrastructure

output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.main.name
}

output "resource_group_location" {
  description = "Location of the resource group"
  value       = azurerm_resource_group.main.location
}

output "storage_account_name" {
  description = "Name of the storage account"
  value       = azurerm_storage_account.iot_storage.name
}

output "storage_account_primary_connection_string" {
  description = "Primary connection string for the storage account"
  value       = azurerm_storage_account.iot_storage.primary_blob_connection_string
  sensitive   = true
}

output "storage_container_name" {
  description = "Name of the storage container"
  value       = azurerm_storage_container.iot_files.name
}

output "iothub_name" {
  description = "Name of the IoT Hub"
  value       = azurerm_iothub.main.name
}

output "iothub_hostname" {
  description = "Hostname of the IoT Hub"
  value       = azurerm_iothub.main.hostname
}

output "iothub_service_connection_string" {
  description = "Service connection string for the IoT Hub (for management operations)"
  value       = "HostName=${azurerm_iothub.main.hostname};SharedAccessKeyName=${azurerm_iothub_shared_access_policy.device_connect.name};SharedAccessKey=${azurerm_iothub_shared_access_policy.device_connect.primary_key}"
  sensitive   = true
}

# Note: Device-specific outputs have been removed since azurerm_iothub_device
# resource is not supported in Terraform. Create devices programmatically using:
# - Azure CLI: az iot hub device-identity create
# - Azure IoT SDK
# - Azure Portal

# Example commands to create a device after deployment:
# az iot hub device-identity create --hub-name <iothub_name> --device-id <device_id>
# az iot hub device-identity connection-string show --hub-name <iothub_name> --device-id <device_id>
