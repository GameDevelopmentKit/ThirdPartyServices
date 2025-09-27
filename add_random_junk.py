#!/usr/bin/env python3
"""
Script to add random junk/dead code to ALL C# methods in Packages/com.gdk.3rd folder.
This version adds junk code to all methods with truly random values.
"""

import os
import re
import random
import string

def generate_random_string(length=None):
    """Generate a random string of letters"""
    if length is None:
        length = random.randint(3, 12)
    return ''.join(random.choices(string.ascii_lowercase, k=length))

def generate_random_junk_code():
    """Generate truly random junk code snippets with completely random values"""
    var_name = generate_random_string(random.randint(4, 8))

    junk_types = [
        f"var {var_name} = {random.randint(-9999, 9999)};",
        f"var {var_name} = \"{generate_random_string()}\";",
        f"var {var_name} = '{random.choice(string.ascii_letters)}';",
        f"var {var_name} = {str(random.choice([True, False])).lower()};",
        f"int {var_name} = {random.randint(-9999, 9999)};",
        f"string {var_name} = \"{generate_random_string(random.randint(5, 15))}\";",
        f"bool {var_name} = {str(random.choice([True, False])).lower()};",
        f"float {var_name} = {round(random.uniform(-999.99, 999.99), 2)}f;",
        f"double {var_name} = {round(random.uniform(-999.999, 999.999), 3)};",
    ]

    return random.choice(junk_types)

def add_junk_to_method_body(method_content, indent_size):
    """Add junk code at the beginning of a method body"""
    lines = method_content.split('\n')

    # Find the opening brace
    brace_index = -1
    for i, line in enumerate(lines):
        if '{' in line:
            brace_index = i
            break

    if brace_index == -1:
        return method_content

    # Add junk code after opening brace
    junk_code = ' ' * indent_size + generate_random_junk_code()
    lines.insert(brace_index + 1, junk_code)

    return '\n'.join(lines)

def process_cs_file_all_methods(file_path):
    """Process a single C# file and add junk to ALL methods"""
    # Only print for every 10th file to reduce output
    file_count = process_cs_file_all_methods.counter
    if file_count % 10 == 0:
        print(f"Processing file #{file_count}: {os.path.basename(file_path)}")
    process_cs_file_all_methods.counter += 1

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()

        # Skip if file is interface
        if re.search(r'\binterface\s+\w+', content):
            return 0

        # Pattern to match all methods (not properties, not abstract, not expression-bodied)
        # This complex regex matches method declarations with bodies
        method_pattern = r'((?:public|private|protected|internal|static|virtual|override|sealed|\s)+(?:async\s+)?(?:Task<[^>]*>|Task|void|bool|int|string|float|double|decimal|byte|char|long|short|IEnumerable<[^>]*>|IList<[^>]*>|List<[^>]*>|Dictionary<[^,]*,[^>]*>|[A-Z]\w*(?:<[^>]*>)?)\s+\w+\s*(?:<[^>]*>)?\s*\([^)]*\)\s*(?:where\s+\w+\s*:\s*[^{]*)?)(\{(?:[^{}]|{[^{}]*})*\})'

        modified_content = content
        matches = list(re.finditer(method_pattern, content, re.MULTILINE | re.DOTALL))
        methods_modified = 0

        # Process matches in reverse order to maintain string positions
        for match in reversed(matches):
            method_signature = match.group(1)
            method_body = match.group(2)

            # Skip if it's expression-bodied or abstract
            if '=>' in method_signature or 'abstract ' in method_signature:
                continue

            # Skip if method body is too small (likely a property getter/setter)
            if len(method_body.strip()) < 10:
                continue

            # Check if this method already has junk code (avoid duplicates)
            if re.search(r'var\s+[a-z]{4,8}\s*=', method_body[:100]):
                continue

            # Determine indentation
            method_lines = (method_signature + method_body).split('\n')
            indent = 0
            for line in method_lines:
                if '{' in line:
                    indent = len(line) - len(line.lstrip()) + 4
                    break

            # Add junk code at the beginning of method body
            modified_body = add_junk_to_method_body(method_body, indent)

            # Replace in the content
            start_pos = match.start(2)
            end_pos = match.end(2)
            modified_content = modified_content[:start_pos] + modified_body + modified_content[end_pos:]
            methods_modified += 1

        # Also handle empty methods with { } on same line
        empty_method_pattern = r'((?:public|private|protected|internal|static|virtual|override|sealed|\s)+(?:async\s+)?(?:Task<[^>]*>|Task|void|bool|int|string|float|double|decimal|byte|char|long|short|IEnumerable<[^>]*>|IList<[^>]*>|List<[^>]*>|Dictionary<[^,]*,[^>]*>|[A-Z]\w*(?:<[^>]*>)?)\s+\w+\s*(?:<[^>]*>)?\s*\([^)]*\))\s*\{\s*\}'

        empty_matches = list(re.finditer(empty_method_pattern, modified_content))
        for match in reversed(empty_matches):
            method_signature = match.group(1)

            # Skip if it's abstract or expression-bodied
            if 'abstract ' in method_signature or '=>' in method_signature:
                continue

            # Calculate indentation
            match_lines = match.group(0).split('\n')
            indent = len(match_lines[0]) - len(match_lines[0].lstrip())

            # Generate junk code
            junk_code = ' ' * (indent + 4) + generate_random_junk_code()

            # Replace { } with properly formatted method
            replacement = f"{method_signature}\n{' ' * indent}{{\n{junk_code}\n{' ' * indent}}}"

            modified_content = modified_content[:match.start()] + replacement + modified_content[match.end():]
            methods_modified += 1

        # Write the modified content back to the file
        if methods_modified > 0:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(modified_content)

        return methods_modified

    except Exception as e:
        print(f"[ERROR] Error processing {file_path}: {str(e)}")
        return 0

# Initialize counter for the function
process_cs_file_all_methods.counter = 1

def main():
    """Main function to add junk code to all C# files in Packages/com.gdk.3rd"""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    target_folder = script_dir

    if not os.path.exists(target_folder):
        print(f"Error: Target folder '{target_folder}' not found!")
        return

    # Find all .cs files in the target folder
    cs_files = []
    for root, dirs, files in os.walk(target_folder):
        for file in files:
            if file.endswith('.cs') and not file.endswith('.py'):
                cs_files.append(os.path.join(root, file))

    print(f"Found {len(cs_files)} C# files to process...")
    print("=" * 50)

    # Process each file
    total_methods = 0
    files_modified = 0
    for cs_file in cs_files:
        methods = process_cs_file_all_methods(cs_file)
        total_methods += methods
        if methods > 0:
            files_modified += 1

    print("=" * 50)
    print("[COMPLETE] Random junk code addition completed!")
    print(f"Modified {total_methods} methods across {files_modified} files")
    print(f"Total files processed: {len(cs_files)} in Packages/com.gdk.3rd")

if __name__ == "__main__":
    main()