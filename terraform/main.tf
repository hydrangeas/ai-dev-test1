# Configure the Azure Provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.4"
    }
  }
}

# Configure the Microsoft Azure Provider
provider "azurerm" {
  features {}
}

# Generate random suffix for unique resource names
resource "random_string" "suffix" {
  length  = 8
  special = false
  upper   = false
}

# Create a resource group
resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "AiDevTest1"
    ManagedBy   = "Terraform"
  }
}

# Create a storage account for IoT Hub file uploads
resource "azurerm_storage_account" "iot_storage" {
  name                     = "${var.storage_account_prefix}${random_string.suffix.result}"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = var.storage_replication_type

  # Enable blob storage features needed for IoT Hub
  blob_properties {
    cors_rule {
      allowed_headers    = ["*"]
      allowed_methods    = ["DELETE", "GET", "HEAD", "MERGE", "POST", "OPTIONS", "PUT"]
      allowed_origins    = ["*"]
      exposed_headers    = ["*"]
      max_age_in_seconds = 3600
    }
  }

  tags = {
    Environment = var.environment
    Project     = "AiDevTest1"
    ManagedBy   = "Terraform"
    Purpose     = "IoTHubFileStorage"
  }
}

# Create a storage container for uploaded files
resource "azurerm_storage_container" "iot_files" {
  name                  = var.storage_container_name
  storage_account_id    = azurerm_storage_account.iot_storage.id
  container_access_type = "private"
}

# Create IoT Hub
resource "azurerm_iothub" "main" {
  name                = "${var.iothub_prefix}${random_string.suffix.result}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location

  sku {
    name     = var.iothub_sku_name
    capacity = var.iothub_sku_capacity
  }

  # Configure file upload settings
  file_upload {
    connection_string = azurerm_storage_account.iot_storage.primary_blob_connection_string
    container_name    = azurerm_storage_container.iot_files.name
    sas_ttl           = "PT1H"

    lock_duration      = "PT1M"
    default_ttl        = "PT1H"
    max_delivery_count = 10
  }

  tags = {
    Environment = var.environment
    Project     = "AiDevTest1"
    ManagedBy   = "Terraform"
  }
}

# Create IoT Hub shared access policy for devices
resource "azurerm_iothub_shared_access_policy" "device_connect" {
  name                = "device-connect"
  resource_group_name = azurerm_resource_group.main.name
  iothub_name         = azurerm_iothub.main.name

  device_connect = true
}

# Create an IoT device
resource "azurerm_iothub_device" "test_device" {
  name                = var.device_id
  iothub_name         = azurerm_iothub.main.name
  resource_group_name = azurerm_resource_group.main.name
  authentication_type = "sas"
}
