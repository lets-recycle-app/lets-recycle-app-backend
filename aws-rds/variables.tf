variable "aws_region" {
    type        = string
    default     = "eu-west-2"
}

variable "rds_instance_identifer" {
    type        = string
    default = "prod-mysql"
}

variable "database_name" {
    type        = string
    default = "recycle"
}

variable "database_username" {
    type        = string
    sensitive = true
    default     = "*"
}

variable "database_password" {
    type        = string
    sensitive = true
    default = "*"
}