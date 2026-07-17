#!/usr/bin/env bash
# Run the periodic NuGet bump and verify it didn't quietly cross the
# net10 shared-framework ceiling on either of the runtime-deployed
# projects.
#
# Depends on https://github.com/dotnet-outdated/dotnet-outdated being installed.
#
# This script was created to help avoid a repeat of an upgrade that failed when
# deployed to a dev environment having passed all CI build & test.
#
# Background: BackendAccountService.Api (ASP.NET Core on App Service)
# and BackendAccountService.ValidationData.Api (Functions in-process v4)
# both deploy onto net10 hosts that pre-load Microsoft.Extensions.* 10.x
# from the shared framework before user code runs. A transitive bump
# pulling in M.Extensions.* >= 11.x will build clean, pass `dotnet test`,
# and fail at startup on Azure with a runtime FileLoadException /
# TypeLoadException. Test runners don't reproduce it because they don't
# load the ASP.NET Core / Functions shared framework.
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
# version pulls in transitives that cross the net10 ceiling. Each entry
# needs a one-line reason; re-evaluate when migrating off net10.
HOLD_BACK=()

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
echo "==> Probing deployed projects for transitive framework-bundled assemblies above net10 ceiling"
hazards=0
for p in "${DEPLOYED_PROJECTS[@]}"; do
    echo "--- $p ---"
    hits=$(dotnet list "$p" package --include-transitive --framework net10.0 \
        | grep -E "Microsoft\.(Extensions|AspNetCore)\..* (11|12|13|14)\." || true)
    if [ -n "$hits" ]; then
        echo "$hits"
        hazards=$((hazards + 1))
    else
        echo "(none)"
    fi
done

echo
if [ "$hazards" -gt 0 ]; then
    echo "FAIL: ${hazards} deployed project(s) crossed the net10 shared-framework ceiling."
    echo "Identify the package that pulled in the 11.x family, add it to HOLD_BACK,"
    echo "revert the csproj changes with \`git restore -- 'src/**/*.csproj'\`, re-run."
    exit 1
fi
echo "OK: upgrade applied, build + tests green, net10 ceiling intact."
