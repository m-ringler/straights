#!/bin/bash
# This script generates .solution files for the grid files found in test/
# The solution files are used in the regression tests.

set -o pipefail
for f in test/*.txt test/*.png test/local/*.txt test/local/*.png; do
	echo "$f"
	sol="${f%.*}.solution"
	if ! Straights/bin/Debug/net9.0/straights solve "$f" | head -n -1 >"$sol"; then
		echo Failed
		rm "$sol"
	fi
done
