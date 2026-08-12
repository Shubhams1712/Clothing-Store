#!/usr/bin/env bash
set -euo pipefail

# Production database migration script
# Reads connection string from ConnectionStrings__DefaultConnection environment variable

DRY_RUN=false
if [[ "${1:-}" == "--dry-run" ]]; then
    DRY_RUN=true
fi

echo "=== Production Database Migration ==="
echo ""

# --- Validate environment ---
if [[ -z "${ConnectionStrings__DefaultConnection:-}" ]]; then
    echo "ERROR: ConnectionStrings__DefaultConnection is not set." >&2
    echo "Set it before running this script:" >&2
    echo '  export ConnectionStrings__DefaultConnection="Host=...;Database=...;Username=...;Password=..."' >&2
    exit 1
fi

# Safety check: prevent running against localhost in production
if [[ "${ConnectionStrings__DefaultConnection}" == *"localhost"* ]] && [[ "${ASPNETCORE_ENVIRONMENT:-}" == "Production" ]]; then
    echo "WARNING: Connection string contains 'localhost' but ASPNETCORE_ENVIRONMENT is Production." >&2
    read -rp "Continue anyway? (y/N) " confirm
    if [[ "$confirm" != "y" ]]; then
        echo "Aborted."
        exit 1
    fi
fi

echo "Connection: [configured]"
echo "Environment: ${ASPNETCORE_ENVIRONMENT:-not set}"
echo ""

# --- List pending migrations ---
echo "Checking for pending migrations..."
dotnet ef migrations list --project ../Infrastructure --startup-project ../API
echo ""

if [[ "$DRY_RUN" == true ]]; then
    echo "[DRY RUN] Would apply pending migrations above."
    exit 0
fi

# --- Apply migrations ---
echo "Applying pending migrations..."
dotnet ef database update --project ../Infrastructure --startup-project ../API
echo ""

# --- Verify no pending migrations remain ---
echo "Verifying no pending migrations remain..."
dotnet ef migrations list --project ../Infrastructure --startup-project ../API
echo ""

echo "=== Done ==="
