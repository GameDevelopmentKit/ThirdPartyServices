#!/usr/bin/env python3
"""
Remove incorrectly placed junk code that causes compilation errors.
"""

import os
import re

def clean_file(file_path):
    """Remove lines that are incorrectly placed junk code"""
    print(f"Cleaning: {os.path.basename(file_path)}")

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()

        cleaned_lines = []
        removed_count = 0

        for i, line in enumerate(lines):
            # Pattern for incorrectly placed junk code (at class level with wrong indentation)
            if re.match(r'^(\s{12}|\s{8})var\s+[a-z]{4,8}\s*=.*;\s*$', line):
                print(f"  Removing line {i+1}: {line.strip()}")
                removed_count += 1
                continue
            if re.match(r'^(\s{12}|\s{8})(int|string|bool|float|double)\s+[a-z]{4,8}\s*=.*;\s*$', line):
                print(f"  Removing line {i+1}: {line.strip()}")
                removed_count += 1
                continue

            cleaned_lines.append(line)

        if removed_count > 0:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(cleaned_lines)
            print(f"  Removed {removed_count} incorrectly placed junk lines")

        return removed_count

    except Exception as e:
        print(f"  [ERROR] {str(e)}")
        return 0

def main():
    """Clean all files that have compilation errors"""

    script_dir = os.path.dirname(os.path.abspath(__file__))

    # Files with known errors
    problem_files = [
        "ServiceImplementation/AdsServices/AdMob/AdMobAdService.cs",
        "ServiceImplementation/AdsServices/AppLovin/AppLovinAdsWrapper.cs",
    ]

    total_removed = 0
    for file_name in problem_files:
        file_path = os.path.join(script_dir, file_name)
        if os.path.exists(file_path):
            removed = clean_file(file_path)
            total_removed += removed
        else:
            print(f"File not found: {file_name}")

    print("=" * 50)
    print(f"[COMPLETE] Removed {total_removed} incorrectly placed junk lines")

if __name__ == "__main__":
    main()