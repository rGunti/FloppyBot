#!/bin/bash
source /semver.sh

MAJOR=0  
MINOR=0  
PATCH=0  
SPECIAL=""

semverParseInto "$(cat /app-version)" MAJOR MINOR PATCH SPECIAL

echo MAJOR=${MAJOR}
echo MINOR=${MINOR}
echo PATCH=${PATCH}
echo SPECIAL=${SPECIAL}

echo "${MAJOR}.${MINOR}.${PATCH}" > /app-short-version
