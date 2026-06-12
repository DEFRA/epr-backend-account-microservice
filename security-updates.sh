#!/usr/bin/env bash
# Run the periodic NuGet bump and verify it didn't quietly cross the
# net8 shared-framework ceiling on either of the runtime-deployed
# projects.
#
# Depends on https://github.com/dotnet-outdated/dotnet-outdated being installed.
#
# This script was created to help avoid a repeat of an upgrade that failed when
# deployed to a dev environment having passed all CI build & test.
#
# Background: BackendAccountService.Api (ASP.NET Core on App Service)
# and BackendAccountService.ValidationData.Api (Functions in-process v4)
# both deploy onto net8 hosts that pre-load Microsoft.Extensions.* 8.x
# from the shared framework before user code runs. A transitive bump
# pulling in M.Extensions.* >= 10.x (typically via Azure.Identity 1.21
# -> Azure.Core 1.53 -> Microsoft.Extensions.Hosting.Abstractions 10)
# will build clean, pass `dotnet test`, and fail at startup on Azure
# with `Could not load file or assembly 'Microsoft.Extensions.Options,
# Version=10.0.0.0'`. Test runners don't reproduce it because they
# don't load the ASP.NET Core / Functions shared framework.
#
# Test projects and console tools are unaffected (no shared framework
# pre-load), so the check is scoped to the two deployed projects only.

set -euo pipefail

SLN=src/BackendAccountService.sln
DEPLOYED_PROJECTS=(
    src/BackendAccountService.Api
    src/BackendAccountService.ValidationData.Api
)

# Packages held back from `--upgrade` because the latest within-major
# version pulls in transitives that cross the net8 ceiling. Each entry
# needs a one-line reason; re-evaluate when migrating off net8.
HOLD_BACK=(
    Azure.Identity  # 1.21+ pulls Microsoft.Extensions.* 10.x via Azure.Core 1.53
)

EXCLUDE_FLAGS=()
for pkg in "${HOLD_BACK[@]}"; do
    EXCLUDE_FLAGS+=(--exclude "$pkg")
done

echo "==> dotnet outdated --version-lock Major --upgrade (excluding held-back packages)"
dotnet outdated --version-lock Major --upgrade "${EXCLUDE_FLAGS[@]}" "$SLN"

echo
echo "==> dotnet build"
dotnet build "$SLN"

echo
echo "==> dotnet test"
dotnet test "$SLN" --no-build

echo
echo "==> Probing deployed projects for transitive framework-bundled assemblies above net8 ceiling"
hazards=0
for p in "${DEPLOYED_PROJECTS[@]}"; do
    echo "--- $p ---"
    hits=$(dotnet list "$p" package --include-transitive --framework net8.0 \
        | grep -E "Microsoft\.(Extensions|AspNetCore)\..* (10|11|12|13|14)\." || true)
    if [ -n "$hits" ]; then
        echo "$hits"
        hazards=$((hazards + 1))
    else
        echo "(none)"
    fi
done

echo
if [ "$hazards" -gt 0 ]; then
    echo "FAIL: ${hazards} deployed project(s) crossed the net8 shared-framework ceiling."
    echo "Identify the package that pulled in the 10.x family, add it to HOLD_BACK,"
    echo "revert the csproj changes with \`git restore -- 'src/**/*.csproj'\`, re-run."
    exit 1
fi
echo "OK: upgrade applied, build + tests green, net8 ceiling intact."
