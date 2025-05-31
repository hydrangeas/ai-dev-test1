# Azure IoT Hub Infrastructure with Terraform

このディレクトリには、Azure IoT HubとStorage Accountを作成するためのTerraformファイルが含まれています。

## 前提条件

- [Terraform](https://terraform.io/) v1.0以降がインストールされていること
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/) がインストールされ、ログインしていること
- 適切なAzureサブスクリプションへのアクセス権限があること

## ファイル構成

```txt
.terraform/
├── main.tf           # メインのリソース定義
├── variables.tf      # 変数定義
├── outputs.tf        # 出力値定義
├── terraform.tfvars  # 変数の値設定（要設定）
└── README.md         # このファイル
```

## セットアップ手順

### 1. Azure CLIでログイン

```bash
az login
az account set --subscription "your-subscription-id"
```

### 2. terraform.tfvarsファイルの編集

`terraform.tfvars`ファイルを開き、環境に応じて値を編集してください：

```hcl
# Resource Group Configuration
resource_group_name = "rg-aidevtest1"
location            = "Japan East"
environment         = "dev"

# Storage Account Configuration
storage_account_prefix   = "staidevtest1"     # 3-15文字、英数字のみ
storage_replication_type = "LRS"
storage_container_name   = "iot-uploads"

# IoT Hub Configuration
iothub_prefix       = "iot-aidevtest1"        # 3-40文字
iothub_sku_name     = "F1"                    # F1(無料), S1, S2, S3
iothub_sku_capacity = 1

# Device Configuration
device_id = "test-device-001"                 # 1-128文字
```

### 3. Terraformの初期化

```bash
cd .terraform
terraform init
```

### 4. 実行計画の確認

```bash
terraform plan
```

### 5. リソースの作成

```bash
terraform apply
```

確認プロンプトで "yes" を入力してリソースを作成します。

## 作成されるリソース

1. **Resource Group** - すべてのリソースのコンテナ
2. **Storage Account** - IoT Hubファイルアップロード用（LRS、一意の名前）
3. **Storage Container** - アップロードファイル保存用コンテナ
4. **IoT Hub** - デバイス接続とファイルアップロード機能（F1無料プラン）
5. **IoT Device** - テスト用のデバイス登録
6. **Shared Access Policy** - デバイス接続用のポリシー

## 出力値の確認

デプロイ後、以下のコマンドで重要な値を確認できます：

```bash
# すべての出力値を表示
terraform output

# 特定の出力値を表示（センシティブ値含む）
terraform output -json

# デバイス接続文字列を表示
terraform output iothub_device_connection_string
```

## アプリケーション設定の更新

Terraform実行後、出力されたデバイス接続文字列を`appsettings.json`に設定してください：

```bash
# 接続文字列を取得
terraform output -raw iothub_device_connection_string

# デバイスIDを取得
terraform output -raw device_id
```

`AiDevTest1.WpfApp/appsettings.json`を以下のように更新：

```json
{
  "AuthInfo": {
    "ConnectionString": "HostName=iot-aidevtest1xxxxxxxx.azure-devices.net;DeviceId=test-device-001;SharedAccessKey=xxxxxxxxxx",
    "DeviceId": "test-device-001"
  }
}
```

## リソースの削除

**注意**: この操作はすべてのリソースを削除します。データは復旧できません。

```bash
terraform destroy
```

## よくある問題と解決方法

### 1. リソース名の競合

Storage AccountやIoT Hubの名前が既に使用されている場合：

- `terraform.tfvars`でプレフィックスを変更してください
- ランダムサフィックスが自動的に追加されます

### 2. 権限エラー

リソース作成に失敗する場合：

- Azure CLIで適切なサブスクリプションにログインしているか確認
- Contributorまたは相当の権限があるか確認

### 3. IoT Hub無料プランの制限

F1プランは以下の制限があります：

- 1サブスクリプションあたり1つまで
- 1日8,000メッセージまで
- 既にF1 IoT Hubがある場合は、S1プランに変更してください

## 参考リンク

- [Azure IoT Hub価格](https://azure.microsoft.com/ja-jp/pricing/details/iot-hub/)
- [Azure Storage価格](https://azure.microsoft.com/ja-jp/pricing/details/storage/)
- [Terraform Azure Provider](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
