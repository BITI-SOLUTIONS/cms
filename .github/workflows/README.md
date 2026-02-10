# GitHub Actions CI/CD Pipeline Documentation

This directory contains the GitHub Actions workflows for automated build and deployment of the CMS application.

## 📋 Available Workflows

### 1. Build and Deploy API (`build-and-deploy-api.yml`)

**Trigger:** Automatically runs on push to `main` branch when there are changes in:
- `CMS.API/**`
- `CMS.Application/**`
- `CMS.Data/**`
- `CMS.Entities/**`
- `CMS.API/Dockerfile`
- `connectionstrings.json`

**What it does:**
1. Builds the API Docker image with .NET 9.0
2. Pushes to Harbor registry with tags `latest` and `{commit-sha}`
3. Deploys to Kubernetes cluster (namespace: `cms`)
4. Performs health check at `http://cms.biti-solutions.com/health`

### 2. Build and Deploy UI (`build-and-deploy-ui.yml`)

**Trigger:** Automatically runs on push to `main` branch when there are changes in:
- `CMS.UI/**`
- `CMS.Application/**`
- `CMS.Data/**`
- `CMS.Entities/**`
- `Dockerfile` (root Dockerfile builds the UI)
- `connectionstrings.json`

**What it does:**
1. Builds the UI Docker image with .NET 9.0
2. Pushes to Harbor registry with tags `latest` and `{commit-sha}`
3. Deploys to Kubernetes cluster (namespace: `cms`)
4. Performs health check at `https://cms.biti-solutions.com`

### 3. Build and Deploy Full Stack (`build-and-deploy-full.yml`)

**Trigger:**
- Automatically runs on any push to `main` branch
- Can be triggered manually via GitHub Actions UI

**What it does:**
1. Builds both API and UI Docker images in parallel
2. Pushes both images to Harbor registry
3. Deploys both to Kubernetes cluster in parallel
4. Performs comprehensive health checks on both services

## 🔐 Required GitHub Secrets

You must configure these secrets in your repository before the workflows can run:

**Go to:** Repository Settings → Secrets and variables → Actions → New repository secret

| Secret Name | Description | Example |
|------------|-------------|---------|
| `HARBOR_REGISTRY` | Harbor registry URL | `registry.biti-solutions.com` |
| `HARBOR_USERNAME` | Harbor registry username | `root` |
| `HARBOR_PASSWORD` | Harbor registry password | Your Harbor password |
| `KUBECONFIG_BASE64` | Base64-encoded kubeconfig file | See instructions below |

### How to Get KUBECONFIG_BASE64

> **Security Best Practices**: 
> - Use SSH key-based authentication (never password-based)
> - Consider using a non-root user with sudo privileges
> - Ensure proper firewall rules and IP whitelisting are in place
> - Store server access details securely (e.g., in a password manager)
> - Follow your organization's security policies for production server access

1. SSH into your Kubernetes server (ensure you have proper SSH key configured):
   ```bash
   ssh root@147.182.204.86
   ```

2. Get the base64-encoded kubeconfig:
   ```bash
   cat ~/.kube/config | base64 -w 0
   ```

3. Copy the entire output and paste it as the value for `KUBECONFIG_BASE64` secret in GitHub

**Alternative (from your local machine if you have the kubeconfig):**
```bash
cat /path/to/your/kubeconfig | base64 -w 0
```

## 🚀 Manual Deployments

### Option 1: Using GitHub Actions UI

1. Go to the **Actions** tab in your repository
2. Select **Build and Deploy Full Stack** workflow
3. Click **Run workflow** button
4. Select the `main` branch
5. Click **Run workflow**

### Option 2: Using Git Push

Simply push your changes to the `main` branch:
```bash
git add .
git commit -m "Your changes"
git push origin main
```

The appropriate workflow will automatically trigger based on which files were changed.

### Important Notes

- **UI Dockerfile**: The root `Dockerfile` is used for building the UI component (not `Dockerfile.UI`)
- **API Dockerfile**: Located at `CMS.API/Dockerfile`
- Both use .NET 9.0 SDK and runtime

## 📊 Monitoring Deployments

### View Workflow Status
1. Go to the **Actions** tab in your repository
2. Click on any workflow run to see detailed logs
3. Each job (build, deploy, health-check) can be expanded to see individual steps

### Check Deployment Status in Kubernetes

> **Note**: Ensure you have proper authentication and authorization before accessing the production server.

```bash
# SSH into the server (use your configured SSH key)
ssh root@147.182.204.86

# Check pod status
kubectl get pods -n cms

# Check deployment status
kubectl get deployments -n cms

# Check deployment logs
kubectl logs -n cms deployment/cms-api-deployment
kubectl logs -n cms deployment/cms-ui-deployment

# Check deployment details
kubectl describe deployment cms-api-deployment -n cms
kubectl describe deployment cms-ui-deployment -n cms
```

## 🔧 Troubleshooting

### Build Failures

**Problem:** Docker build fails with "unauthorized" error

**Solution:** Check that your Harbor credentials are correct:
1. Verify `HARBOR_REGISTRY`, `HARBOR_USERNAME`, and `HARBOR_PASSWORD` secrets
2. Test login manually:
   ```bash
   docker login registry.biti-solutions.com -u root
   ```

---

**Problem:** Build fails with .NET version errors

**Solution:** 
- Ensure both `Dockerfile` and `CMS.API/Dockerfile` use .NET 9.0
- Check project files (`.csproj`) have correct `TargetFramework`

### Deploy Failures

**Problem:** kubectl cannot connect to cluster

**Solution:** 
1. Verify `KUBECONFIG_BASE64` secret is correctly set
2. Test the kubeconfig locally:
   ```bash
   echo "$KUBECONFIG_BASE64" | base64 -d > test-kubeconfig
   kubectl --kubeconfig=test-kubeconfig get nodes
   ```
3. Ensure the kubeconfig has proper permissions and is not expired

---

**Problem:** Deployment times out

**Solution:**
1. Check pod status: `kubectl get pods -n cms`
2. Check pod logs: `kubectl logs -n cms <pod-name>`
3. Common issues:
   - Image pull errors (check Harbor credentials in Kubernetes)
   - Application startup errors (check logs)
   - Resource limits (check if cluster has enough resources)

---

**Problem:** Pods are in `ImagePullBackOff` state

**Solution:**
1. Check that the Kubernetes cluster can access Harbor registry
2. Verify image pull secret is configured in the namespace:
   ```bash
   kubectl get secrets -n cms
   ```
3. If missing, create it:
   ```bash
   kubectl create secret docker-registry harbor-registry \
     --docker-server=registry.biti-solutions.com \
     --docker-username=root \
     --docker-password=<password> \
     --namespace=cms
   ```
4. Ensure deployments reference the secret:
   ```yaml
   imagePullSecrets:
   - name: harbor-registry
   ```

### Health Check Failures

**Problem:** Health check fails but deployment succeeded

**Solution:**
1. Wait a bit longer (app might still be starting)
2. Check if the health endpoint exists:
   ```bash
   curl -v http://cms.biti-solutions.com/health
   ```
3. Check service and ingress configuration:
   ```bash
   kubectl get svc -n cms
   kubectl get ingress -n cms
   ```

---

**Problem:** Curl fails with connection errors

**Solution:**
1. Verify DNS resolves correctly: `nslookup cms.biti-solutions.com`
2. Check ingress controller is running:
   ```bash
   kubectl get pods -n ingress-nginx
   ```
3. Verify service endpoints:
   ```bash
   kubectl get endpoints -n cms
   ```

## 🎯 Best Practices

1. **Always check the Actions tab** after pushing to see if your deployment succeeded
2. **Use manual workflow** for testing before pushing to main
3. **Monitor the first few automated deployments** to ensure everything works correctly
4. **Check health endpoints** in your browser after deployment
5. **Keep secrets secure** - never commit them to the repository
6. **Review logs** if anything fails - they usually contain helpful error messages

## 📚 Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [Kubectl Setup Action](https://github.com/azure/setup-kubectl)
- [Harbor Documentation](https://goharbor.io/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

## 🆘 Getting Help

If you encounter issues not covered here:

1. Check the **Actions** tab for detailed error logs
2. SSH into the server and check Kubernetes pod logs
3. Review this troubleshooting guide
4. Check if Harbor registry is accessible
5. Verify all secrets are correctly configured

## ✅ Verification Checklist

Before expecting workflows to work:

- [ ] All required secrets are configured in GitHub
- [ ] `KUBECONFIG_BASE64` is valid and base64-encoded correctly
- [ ] Harbor registry is accessible from GitHub Actions runners
- [ ] Kubernetes cluster is accessible with the provided kubeconfig
- [ ] Kubernetes namespace `cms` exists
- [ ] Deployments `cms-api-deployment` and `cms-ui-deployment` exist in the cluster
- [ ] Image pull secrets are configured in Kubernetes namespace
- [ ] DNS records for `cms.biti-solutions.com` are configured correctly
- [ ] Ingress is configured to route traffic to the services
