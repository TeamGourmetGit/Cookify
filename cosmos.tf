resource "azurerm_cosmosdb_account" "CDBA-Cookify" {
  name                = "cookify-cosmos-db-1"
  location            = local.RGlocation
  resource_group_name = local.RGname
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = local.RGlocation
    failover_priority = 0
  }

  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet ]
}


resource "azurerm_cosmosdb_sql_database" "SQLDB-Cookify" {
  name                = "SQLDB-Cookify"
  resource_group_name = local.RGname
  account_name        = azurerm_cosmosdb_account.CDBA-Cookify.name
  

  depends_on = [ azurerm_cosmosdb_account.CDBA-Cookify ]
}

resource "azurerm_cosmosdb_sql_container" "recipe-container" {
  name                  = "recipe-container"
  resource_group_name   = local.RGname
  account_name          = azurerm_cosmosdb_account.CDBA-Cookify.name
  database_name         = azurerm_cosmosdb_sql_database.SQLDB-Cookify.name
  partition_key_path    = "/definition/id"
  partition_key_version = 1
  throughput            = 400
  

  indexing_policy {
    indexing_mode = "consistent"

    included_path {
      path = "/*"
    }

    included_path {
      path = "/included/?"
    }

    excluded_path {
      path = "/excluded/?"
    }
  }

  unique_key {
    paths = ["/definition/idlong", "/definition/idshort"]
  }

  depends_on = [ azurerm_cosmosdb_sql_database.SQLDB-Cookify ]
}