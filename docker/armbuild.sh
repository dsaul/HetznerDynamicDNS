#!/usr/bin/env bash
set -o errexit
set -o nounset
#set -o xtrace
set -o pipefail

# Change Directory to script directory.
cd $(dirname "${BASH_SOURCE[0]}")

# This doesn't work currently

docker run --privileged --rm tonistiigi/binfmt --install all || true
docker buildx use default
docker buildx build --no-cache --platform arm64 -f dockerfile.txt -t hetzner-dynamic-dns .
docker save hetzner-dynamic-dns > hetzner-dynamic-dns.tar