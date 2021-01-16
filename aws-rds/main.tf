terraform {
    required_providers {
        aws = {
            source    = "hashicorp/aws"
            version   = "~> 3.0"
        }
    }
}

provider "aws" {
    region    = var.aws_region
    profile   = "terraform"
}

// store terraform state in S3
terraform {
  backend "s3" {
    bucket = "terraform-recycle"
    key    = "http://terraform-recycle.s3.amazonaws.com/"
    region = "eu-west-2"
  }
}

data "aws_vpc" "default" {
    default = true
}

resource "aws_security_group" "mysql_public_access" {
  name        = "RDS MySQL Access"
  description = "Allow access to port 3306"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    description = "Access to MySQL"
    from_port   = 3306
    to_port     = 3306
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1" # All protocols
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "MySQL Ingress"
  }
}

resource "aws_db_instance" "default" {
  allocated_storage         = 20
  max_allocated_storage     = 0 # disabled autoscaling
  multi_az                  = false
  storage_type              = "gp2"
  engine                    = "mysql"
  engine_version            = "5.7.31"
  instance_class            = "db.t2.micro"
  identifier                = var.rds_instance_identifer
  name                      = var.database_name
  username                  = var.database_username
  password                  = var.database_password
  parameter_group_name      = "default.mysql5.7"

  publicly_accessible       = true
  skip_final_snapshot       = true

  vpc_security_group_ids    = [aws_security_group.mysql_public_access.id]
}