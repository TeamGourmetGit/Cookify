variable "office365DisplayName" {
    description = "Please provide your outlook 365 account id (No Password)"
    type = string
    sensitive = false
}
variable "scaleSetEmail" {
    description = "Please provide an email that the scaleset will send an alarm to if the app needs to scale up"
    type = string
    sensitive = false
}
