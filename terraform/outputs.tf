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

output "iothub_device_connection_string" {
  description = "Connection string for the IoT device"
  value       = "HostName=${azurerm_iothub.main.hostname};DeviceId=${azurerm_iothub_device.test_device.name};SharedAccessKey=${azurerm_iothub_device.test_device.primary_key}"
  sensitive   = true
}

output "iothub_service_connection_string" {
  description = "Service connection string for the IoT Hub (for management operations)"
  value       = "HostName=${azurerm_iothub.main.hostname};SharedAccessKeyName=${azurerm_iothub_shared_access_policy.device_connect.name};SharedAccessKey=${azurerm_iothub_shared_access_policy.device_connect.primary_key}"
  sensitive   = true
}

output "device_id" {
  description = "IoT device ID"
  value       = azurerm_iothub_device.test_device.name
}

output "device_primary_key" {
  description = "Primary key for the IoT device"
  value       = azurerm_iothub_device.test_device.primary_key
  sensitive   = true
}

output "device_secondary_key" {
  description = "Secondary key for the IoT device"
  value       = azurerm_iothub_device.test_device.secondary_key
  sensitive   = true
}

# Configuration for appsettings.json
output "appsettings_configuration" {
  description = "Configuration values for appsettings.json"
  value = {
    AuthInfo = {
      ConnectionString = "HostName=${azurerm_iothub.main.hostname};DeviceId=${azurerm_iothub_device.test_device.name};SharedAccessKey=${azurerm_iothub_device.test_device.primary_key}"
      DeviceId         = azurerm_iothub_device.test_device.name
    }
  }
  sensitive = true
}
