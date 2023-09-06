resource "azurerm_service_plan" "ASP-Cookify" {
  name                = "ASP-Cookify"
  resource_group_name = local.RGname
  location            = local.RGlocation
  sku_name            = "S1"
  os_type             = "Linux"
  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet]
}

resource "azurerm_linux_web_app" "WA-Cookify" {
  name                = "WA-Cookify"
  resource_group_name = local.RGname
  location            = local.RGlocation
  service_plan_id     = azurerm_service_plan.ASP-Cookify.id

  site_config {
    application_stack {
      dotnet_version = "7.0"
    }
  }

  connection_string {
    name  = "Database"
    type  = "Custom"
    value = azurerm_cosmosdb_account.CDBA-Cookify.connection_strings[0]
  }

  depends_on = [ azurerm_service_plan.ASP-Cookify, azurerm_cosmosdb_account.CDBA-Cookify ]
}


resource "azurerm_linux_web_app_slot" "dev-slot" {
  name           = "dev-slot"
  app_service_id = azurerm_linux_web_app.WA-Cookify.id

  site_config {
    application_stack {
      dotnet_version = "7.0"
    }
  }

  depends_on = [ azurerm_linux_web_app.WA-Cookify ]
}