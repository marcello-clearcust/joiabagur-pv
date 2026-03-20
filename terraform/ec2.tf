# ─── Security Group ──────────────────────────────────────────────────────────

resource "aws_security_group" "ec2" {
  name        = "jpv-ec2-sg"
  description = "JoiabagurPV EC2 — allows HTTP/HTTPS inbound, all outbound"

  ingress {
    description = "HTTP (redirect to HTTPS)"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "jpv-ec2-sg" }
}

# ─── EC2 Instance ─────────────────────────────────────────────────────────────

resource "aws_instance" "api" {
  ami                    = var.ami_id
  instance_type          = var.ec2_instance_type
  iam_instance_profile   = aws_iam_instance_profile.ec2.name
  vpc_security_group_ids = [aws_security_group.ec2.id]
  key_name               = var.key_pair_name != "" ? var.key_pair_name : null

  root_block_device {
    volume_type = "gp3"
    volume_size = 20
    encrypted   = true
  }

  user_data = base64encode(templatefile("${path.module}/templates/user_data.sh", {
    domain_name = var.domain_name
  }))

  tags = { Name = "jpv-api-prod" }

  lifecycle {
    # Changing user_data would force instance recreation — not desired after first boot.
    # Re-run the setup script manually via SSM if needed.
    ignore_changes = [user_data]
  }
}

# ─── Elastic IP ───────────────────────────────────────────────────────────────

resource "aws_eip" "api" {
  instance = aws_instance.api.id
  domain   = "vpc"
  tags     = { Name = "jpv-api-eip" }
}
