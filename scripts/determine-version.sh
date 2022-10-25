#!/usr/bin/env bash
source /semver.sh

local MAJOR=0  
local MINOR=0  
local PATCH=0  
local SPECIAL=""

semverParseInto "$(cat /app-version)" MAJOR MINOR PATCH SPECIAL

echo MAJOR=${MAJOR}
echo MINOR=${MINOR}
echo PATCH=${PATCH}
echo SPECIAL=${SPECIAL}

echo "${MAJOR}.${MINOR}.${PATCH}" > /app-short-version
