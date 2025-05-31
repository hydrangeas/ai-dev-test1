# Terraform variables configuration for Azure IoT Hub infrastructure
# Copy this file and modify values as needed for your environment

# Resource Group Configuration
resource_group_name = "rg-aidevtest1"
location            = "Japan East"
environment         = "dev"

# Storage Account Configuration
storage_account_prefix   = "staidevtest1"
storage_replication_type = "LRS"
storage_container_name   = "iot-uploads"

# IoT Hub Configuration
iothub_prefix       = "iot-aidevtest1"
iothub_sku_name     = "F1" # F1 is free tier, S1/S2/S3 are paid tiers
iothub_sku_capacity = 1

# Device Configuration
device_id = "test-device-001"
