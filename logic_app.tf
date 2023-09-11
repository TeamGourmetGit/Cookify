data "azurerm_managed_api" "manged_office365" {
  name     = "office365"
  location = local.RGlocation
}

resource "azurerm_api_connection" "office365" {
  name                = "api-office365-data"
  resource_group_name = local.RGname
  managed_api_id      = data.azurerm_managed_api.manged_office365.id
  display_name        = var.office365DisplayName

  lifecycle {
    ignore_changes = [parameter_values]
  }
  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet ]
}

resource "azurerm_logic_app_workflow" "SendEmailWorkFlow" {
  name                = "SendEmailLogicApp"
  location            = local.RGlocation
  resource_group_name = local.RGname
  parameters = {
    "$connections" = jsonencode({
    office365 = {
      connectionId         = azurerm_api_connection.office365.id
      connectionName       = azurerm_api_connection.office365.name
      connectionProperties = {}
      id                   = data.azurerm_managed_api.manged_office365.id
      }
    })
  }
  workflow_parameters = {
    "$connections" = jsonencode({
    defaultValue = {}
    type         = "Object"
    })
  }

  depends_on = [ azurerm_resource_group.RG-Cookify-TeamGourmet ]
}

resource "azurerm_logic_app_trigger_http_request" "HttpReqSendEmail" {
  name         = "HTTPRequest-SendEmail"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  schema = <<SCHEMA
{
    "type": "Request",
    "kind": "Http",
    "inputs": {
      "schema": {
        "type": "object",
        "properties": {
            "Email": {
                "type": "string"
            },
             "SelectedRecipes": {
               "type": "array",
               "items": {
                   "type": "object",
                   "properties": {
                       "Id": {
                          "type": "string"
                        },
                    "PartitionKey": {
                         "type": "string"
                    },
                    "Ingredients": {
                          "type": "array",
                          "items": {
                             "type": "string"
                          }
                    },
                   "Name": {
                       "type": "string"
                    }
            },
            "required": [
                "Id",
                "PartitionKey",
                "Ingredients",
                "Name"
            ]
          }
        }
      }
    }
  }
}
SCHEMA
depends_on = [ azurerm_logic_app_workflow.SendEmailWorkFlow ]
}
output "url" {
    value = azurerm_logic_app_trigger_http_request.HttpReqSendEmail.callback_url
}

resource "azurerm_logic_app_action_custom" "InitializeVariableDate" {
  name = "Initialize_variable"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  body = <<BODY
 {
    "description": "A variable to get the current date and time",
    "inputs": {
        "variables": [
            {
               "name": "CurrentDate",
               "type": "string",
               "value": "@{utcNow('yyyy-MM-ddTHH:mm')}"
            }
        ]
    },
    "runAfter": {},
    "type": "InitializeVariable"
 }
 BODY
depends_on = [ azurerm_logic_app_trigger_http_request.HttpReqSendEmail ]
}

resource "azurerm_logic_app_action_custom" "ParseJSON" {
  name = "Parse_JSON"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  body = <<BODY
 {
    "description": "Parse the HTTP request json body to workable variables",
    "inputs": {
    "content": "@triggerBody()",
    "schema": {
        "type": "object",
        "properties": {
            "Email": {
                "type": "string"
            },
            "SelectedRecipes": {
                "type": "array",
                "items": {
                    "type": "object",
                    "properties": {
                        "Id": {
                            "type": "string"
                        },
                        "PartitionKey": {
                            "type": "string"
                        },
                        "Ingredients": {
                            "type": "array",
                            "items": {
                                "type": "string"
                            }
                        },
                        "Name": {
                            "type": "string"
                        }
                    },
                    "required": [
                        "Id",
                        "PartitionKey",
                        "Ingredients",
                        "Name"
                    ]
                }
            }
        }
    }
 },
    "runAfter": {
      "Initialize_variable": [
        "Succeeded"
      ]
    },
    "type": "ParseJson"
 }
 BODY
depends_on = [ azurerm_logic_app_action_custom.InitializeVariableDate ]
}

resource "azurerm_logic_app_action_custom" "InitializeVariableConCat" {
  name = "Initialize_variable_ConCat"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  body = <<BODY
    {
    "description": "Creates the variable that will hold the concatinated string that the email shall contain. ",
    "inputs": {
        "variables": [
           {
               "name": "ConcatinatedString",
               "type": "string"
           }
        ]
    },
    "runAfter": {
        "Parse_JSON": [
            "Succeeded"
        ]
    },
    "type": "InitializeVariable"
 }
 BODY

depends_on = [ azurerm_logic_app_action_custom.ParseJSON ]
}

resource "azurerm_logic_app_action_custom" "ForEachLoop" {
  name = "For_each"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  body = <<BODY
    {
    "foreach": "@body('Parse_JSON')?['SelectedRecipes']",
    "actions": {
        "Append_to_string_variable": {
            "type": "AppendToStringVariable",
            "inputs": {
                "name": "ConcatinatedString",
                "value": "@concat('Måltid: ', item()?['Name'], '<br />Ingridienser: <br />', join(item()?['Ingredients'], ',<br />'), '<br /><br />')"
            }
        }
    },
    "runAfter": {
        "Initialize_variable_ConCat": [
            "Succeeded"
        ]
    },
    "type": "Foreach"
 }
 BODY

depends_on = [ azurerm_logic_app_action_custom.InitializeVariableConCat ]
}

resource "azurerm_logic_app_action_custom" "SendEmail" {
  name = "Send_an_email_(V2)"
  logic_app_id = azurerm_logic_app_workflow.SendEmailWorkFlow.id

  body = <<BODY
 {
    "runAfter": {
        "For_each": [
            "Succeeded"
        ]
    },
    "type": "ApiConnection",
    "inputs": {
        "host": {
            "connection": {
                "name": "@parameters('$connections')['office365']['connectionId']"
            }
        },
        "method": "post",
        "body": {
            "To": "@triggerBody()?['Email']",
            "Subject": "Här kommer dina recept! - @{variables('CurrentDate')}",
            "Body": "<p>Nedan finns dina sparade recept!</p><br><p>@{variables('ConcatinatedString')}</p>",
            "Importance": "Normal"
        },
        "path": "/v2/Mail"
    }
 }
 BODY

depends_on = [ azurerm_logic_app_action_custom.ForEachLoop ]
}