# Variable definitions for Azure IoT Hub infrastructure

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "rg-aidevtest1"
}

variable "location" {
  description = "Azure region for resources"
  type        = string
  default     = "Japan East"
}

variable "environment" {
  description = "Environment name (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "storage_account_prefix" {
  description = "Prefix for storage account name (random suffix will be added)"
  type        = string
  default     = "staidevtest1"

  validation {
    condition     = can(regex("^[a-z0-9]{3,15}$", var.storage_account_prefix))
    error_message = "Storage account prefix must be 3-15 characters long and contain only lowercase letters and numbers."
  }
}

variable "storage_replication_type" {
  description = "Storage account replication type"
  type        = string
  default     = "LRS"

  validation {
    condition     = contains(["LRS", "GRS", "RAGRS", "ZRS", "GZRS", "RAGZRS"], var.storage_replication_type)
    error_message = "Storage replication type must be one of: LRS, GRS, RAGRS, ZRS, GZRS, RAGZRS."
  }
}

variable "storage_container_name" {
  description = "Name of the storage container for IoT Hub file uploads"
  type        = string
  default     = "iot-uploads"
}

variable "iothub_prefix" {
  description = "Prefix for IoT Hub name (random suffix will be added)"
  type        = string
  default     = "iot-aidevtest1"

  validation {
    condition     = can(regex("^[a-zA-Z0-9-]{3,40}$", var.iothub_prefix))
    error_message = "IoT Hub prefix must be 3-40 characters long and contain only letters, numbers, and hyphens."
  }
}

variable "iothub_sku_name" {
  description = "IoT Hub SKU name"
  type        = string
  default     = "F1"

  validation {
    condition     = contains(["F1", "S1", "S2", "S3"], var.iothub_sku_name)
    error_message = "IoT Hub SKU must be one of: F1 (free), S1, S2, S3."
  }
}

variable "iothub_sku_capacity" {
  description = "IoT Hub SKU capacity (number of units)"
  type        = number
  default     = 1

  validation {
    condition     = var.iothub_sku_capacity >= 1 && var.iothub_sku_capacity <= 200
    error_message = "IoT Hub SKU capacity must be between 1 and 200."
  }
}

variable "device_id" {
  description = "IoT device ID"
  type        = string
  default     = "test-device-001"

  validation {
    condition     = can(regex("^[a-zA-Z0-9-._]{1,128}$", var.device_id))
    error_message = "Device ID must be 1-128 characters long and contain only letters, numbers, hyphens, periods, and underscores."
  }
}
