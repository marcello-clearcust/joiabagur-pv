# ─── Security Group ──────────────────────────────────────────────────────────
# PostgreSQL only reachable from the EC2 instance — not publicly accessible.

resource "aws_security_group" "rds" {
  name        = "jpv-rds-sg"
  description = "JoiabagurPV RDS - PostgreSQL from EC2 only"

  ingress {
    description     = "PostgreSQL from EC2"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ec2.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "jpv-rds-sg" }
}

# ─── Subnet Group (uses default VPC subnets) ─────────────────────────────────

data "aws_subnets" "default" {
  filter {
    name   = "default-for-az"
    values = ["true"]
  }
}

resource "aws_db_subnet_group" "default" {
  name       = "jpv-db-subnet-group"
  subnet_ids = data.aws_subnets.default.ids

  tags = { Name = "jpv-db-subnet-group" }
}

# ─── RDS Instance ─────────────────────────────────────────────────────────────

resource "aws_db_instance" "postgres" {
  identifier        = "jpv-db-prod"
  engine            = "postgres"
  engine_version    = "15"
  instance_class    = var.db_instance_class
  allocated_storage = 20
  storage_type      = "gp2"
  storage_encrypted = true

  db_name  = "jpv"
  username = "postgres"
  password = var.db_password

  db_subnet_group_name   = aws_db_subnet_group.default.name
  vpc_security_group_ids = [aws_security_group.rds.id]

  publicly_accessible    = false
  multi_az               = false
  deletion_protection    = true
  skip_final_snapshot    = false
  final_snapshot_identifier = "jpv-db-final-snapshot"

  backup_retention_period = 7
  backup_window           = "03:00-04:00"
  maintenance_window      = "Mon:04:00-Mon:05:00"

  tags = { Name = "jpv-db-prod" }
}
