# Step 2

```bash
ansible-playbook -i "<VM IP>," setup-vm.yml -u ubuntu --private-key <PATH TO PRIVATE KEY> -e "letsencrypt_email=your-real-email@example.com"
```

ansible-playbook -i "144.24.172.101," setup-vm.yml -u ubuntu --private-key ~/.ssh/oweme-api-vm -e "letsencrypt_email=marcinsz.pv@gmail.com"