resource "azurerm_monitor_action_group" "CookifyBudgetMonitor" {
  name                = "cookifybudgetmonitor"
  resource_group_name = local.RGname
  short_name = "CBM"
  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet ]
}

resource "azurerm_consumption_budget_resource_group" "CookifytRGBudget" {
  name              = "cookifyrgbudget"
  resource_group_id = azurerm_resource_group.RG-Cookify-TeamGourmet.id

  amount     = 1000
  time_grain = "Monthly"

  time_period {
    start_date = "2023-09-01T00:00:00Z"
    end_date   = "2024-09-11T00:00:00Z"
  }

#   filter {
#     dimension {
#       name = "ResourceId"
#       values = [
#         azurerm_monitor_action_group.example.id,
#       ]
#     }

  notification {
    enabled        = true
    threshold      = 70.0
    operator       = "EqualTo"
    threshold_type = "Forecasted"

    contact_emails = [
      "mattias.strom@iths.se"
    ]

    contact_groups = [
      azurerm_monitor_action_group.CookifyBudgetMonitor.id,
    ]

    contact_roles = [
      "Owner",
    ]
  }

  notification {
    enabled   = false
    threshold = 100.0
    operator  = "GreaterThan"

    contact_emails = [
      "mattias.strom@iths.se"
    ]
  }
  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet ]
}