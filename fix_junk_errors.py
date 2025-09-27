#!/usr/bin/env python3
"""
Script to fix incorrectly placed junk code that causes compilation errors.
Removes junk code that was placed at class level instead of inside method bodies.
"""

import os
import re

def fix_cs_file(file_path):
    """Fix a single C# file by removing incorrectly placed junk code"""
    print(f"Fixing: {file_path}")

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()

        fixed_lines = []
        i = 0
        fixes = 0

        while i < len(lines):
            line = lines[i]

            # Check if this line is a method with { return ... } on same line
            if re.search(r'(public|private|protected|internal|static).*\{.*return.*\}', line):
                fixed_lines.append(line)
                # Check if next line looks like incorrectly placed junk code
                if i + 1 < len(lines):
                    next_line = lines[i + 1]
                    # Pattern for junk code at wrong indentation level
                    if re.match(r'\s+(var|int|string|bool|float|double)\s+[a-z]{4,8}\s*=', next_line.strip()):
                        print(f"  Removing incorrectly placed junk at line {i+2}: {next_line.strip()}")
                        fixes += 1
                        i += 2  # Skip the junk line
                        continue

            fixed_lines.append(line)
            i += 1

        if fixes > 0:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(fixed_lines)
            print(f"  Fixed {fixes} issues")

        return fixes

    except Exception as e:
        print(f"  [ERROR] {str(e)}")
        return 0

def main():
    """Main function to fix specific files with errors"""

    # Files with known errors from compilation output
    files_to_fix = [
        "ServiceImplementation/AdsServices/AdMob/AdMobAdService.cs",
        "ServiceImplementation/AdsServices/AppLovin/AppLovinAdsWrapper.cs",
    ]

    script_dir = os.path.dirname(os.path.abspath(__file__))

    total_fixes = 0
    for file_name in files_to_fix:
        file_path = os.path.join(script_dir, file_name)
        if os.path.exists(file_path):
            fixes = fix_cs_file(file_path)
            total_fixes += fixes
        else:
            print(f"File not found: {file_path}")

    print("=" * 50)
    print(f"[COMPLETE] Fixed {total_fixes} issues")

if __name__ == "__main__":
    main()