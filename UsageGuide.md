# FileTree Usage Guide

FileTree is a powerful CLI tool for generating visual directory trees with advanced features: gitignore-style filtering, collapsing, formatting (ASCII/Unicode/Markdown), hidden file highlighting/styling, and system integration (context menus, PATH, aliases).

## Quick Start
```
FileTree install          # Install into system
FileTree .                # Tree of current directory (ASCII by default)
FileTree C:\\Projects --format Markdown --max-depth 3  # Markdown, depth 3
FileTree uninstall        # Uninstall
```

## Workflow
1. **First Run**: `FileTree install`
   - Adds to user PATH.
   - Creates shims: `FileTree.bat`, `FT.bat`.
   - PowerShell aliases: `Set-Alias FT/FileTree`.
   - **Windows**: Context menus (RMB on folder/file/desktop → FileTree / FileTree Customizable).
   - Creates `FileTree.ignore.txt` (global rules), `FileTree.settings.txt` (option defaults).

2. **Usage**:
   - Console: `FileTree` / `FT` from anywhere.
   - RMB → FileTree (auto-pause).

3. **Removal**: `FileTree uninstall` (current user install) or `uninstall-deep` (scans/removes all).

**Linux**: `install` prints manual setup instructions (PATH/aliases).

## Commands
| Command | Description |
|---------|-------------|
| `FileTree [path]` (`scan`, default) | Generate/print tree. Path optional (current dir default). |
| `install` | System integration. |
| `uninstall` | Remove current user installation. |
| `uninstall-deep` | Deep scan/prompt-remove all FileTree entries. |
| `paths` | Show exe/dir/global config paths. |
| `config rules [path/reset/open]` | Manage `FileTree.ignore.txt`. |
| `config settings [path/reset/open]` | Manage `FileTree.settings.txt`. |

**Global Flag**: `--pause-exit` / `-k` (pause before exit, useful in context menus).

## Scan Options (default `scan`)
Options grouped. `-1` = unlimited. `?` = optional. Examples below.

### Limits
| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--max-depth N` | `-d` | Max recursion depth. | -1 |
| `--max-width N` | `-w` | Max items per directory. | -1 |
| `--max-nodes N` | `-n` | Global node cap. | -1 |

### Format & Collapse
| Option | Description | Default |
|--------|-------------|---------|
| `--format F` / `-f` | `Ascii`/`Markdown`/`Unicode`. | Ascii |
| `--collapse-threshold N` | Collapse if >N children. | auto |
| `--collapse-keep-start N` | Show first N before... | 1 |
| `--collapse-keep-end N` | Last N after... | 1 |
| `--collapse-style S` | `Count`/`Simple`/`ByExtension`. | Count |
| `--collapse-from D` | Start from depth D. | 1 |

### Filtering (Modern, gitignore-style)
| Option | Description |
|--------|-------------|
| `--filter-rules` | `*.log,bin/,!src/` (`,` sep, `!` negate). |
| `--filter-file` | Path to `.filetreeignore`. |
| `--global-filter-file` | Global `FileTree.ignore.txt`. |
| `--no-default-filters` | Skip default globals. |
| `--no-app-global-ignore` / `!` | Skip `FileTree.ignore.txt`. |
| `--no-local-filters` | Skip `.filetreeignore` in subdirs. |

**Precedence**: inline > local/global > gitignore.

**Deprecated (Legacy)**: `--include-ext/-i`, `--exclude-ext/-e`, `--include-names`, `--exclude-names` (auto-converted).

### Hidden Files & Ignore
| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--hidden` / `-h` | `-h` | Exclude hidden (dotfiles/Win attr). | true |
| `--highlight-hidden` | Highlight if not excluded. | true |
| `--hidden-style S` | `Prefix`/`Suffix`/`Minimal`. | Suffix |
| `--ignore-empty` | Skip empty folders. | false |
| `--use-gitignore` / `-g` | `.gitignore` if `.git`. | true (if .git) |

### Output & UX
| Option | Short | Description |
|--------|-------|-------------|
| `--copy` / `-c` | `-c` | Copy to clipboard. |
| `--silent` / `-s` | `-s` | No console (with `--copy`). |
| `--show-options` | Print core options. |
| `--show-options-all` | Print all options. |
| `--wait` | Interactive: input until `show`. |
| `--no-default-settings` | Skip `FileTree.settings.txt`. |

## Configuration Files
- **`FileTree.settings.txt`** (next to exe): Default CLI options (one per line).
  ```
  # --max-depth 2
  --format Markdown
  --use-gitignore false
  ```
  `FileTree config settings` → open/reset/path.

- **`FileTree.ignore.txt`**: Global gitignore rules.
  ```
  # *.log
  node_modules/
  bin/
  ```
  `FileTree config rules` → open/reset/path.

- **`.filetreeignore`**: Local in project/subdirs (auto-loaded recursively).

## Usage Examples

### 1. Simple Project Tree
```
FileTree . --max-depth 2
```

### 2. Markdown for GitHub PR/README/Presentations
```
FileTree src/ --format Markdown --max-width 20 --collapse-threshold 10 > tree.md
```
```
src/
├── FileTree.CLI/
│   ├── Program.cs
│   └── CommandLineOptions.cs
└── FileTree.Core/
    └── ... (15 more files)
```
Analyze this .NET project structure for refactoring.

### 3. LLM Prompt (Project Structure)
```
FileTree . --format Unicode --filter-rules 'node_modules/,*.log,bin/' --copy
```
Paste clipboard to ChatGPT: "Here's my project structure: [paste]. Suggest improvements."

### 4. Filter Junk (node_modules/build/logs)
```
FileTree . --filter-rules 'node_modules/,bin/,obj/,*.log,!.git/'
```

### 5. Source Files Only (CS/JS)
```
FileTree . --filter-rules '*.cs,*.js,*.ts' --use-gitignore true
```

### 6. Collapse Large Dirs
```
FileTree node_modules/ --collapse-from 1 --collapse-style ByExtension --max-depth 3
```

```
node_modules/
├── ... (500 more .js files)
└── react/
    └── ... (120 more files)
```

### 7. Interactive Mode
```
FileTree . --wait
> --max-depth 2 -f Markdown --copy
> show
```

### 8. Context Menu (RMB on Folder)
- **FileTree**: Auto-tree + pause.
- **FileTree Customizable**: `--wait --pause-exit` (tweak interactively).

### 9. Presentations (Unicode)
```
FileTree C:\\Projects\\Demo --format Unicode --highlight-hidden --collapse-keep-start 3 --copy
```

### 10. Full Analysis (No Limits)
```
FileTree . --max-depth -1 --max-width -1 --max-nodes -1 --hidden false --show-options-all
```

### 11. Folders Only
```
FileTree . --filter-rules '**/*,**/'  # Or legacy exclude files
```

## Platform Notes
- **Windows**: Full auto-integration (registry/PATH/profiles). `uninstall-deep` safely cleans.
- **Linux**: `install` shows manual steps:
  ```
  echo 'alias FileTree=/path/to/FileTree.exe' >> ~/.bashrc
  export PATH=$PATH:/path/to/dir
  ```
- **PATH not updated?** Re-login / `refreshenv` (Chocolatey).

## Troubleshooting
| Issue | Solution |
|-------|----------|
| `command not found` | `FileTree install`, re-login. |
| No context menu | `FileTree install`, restart explorer.exe. |
| Filters not working | `FileTree paths`, `config rules`. |
| Legacy options | Auto-converted; migrate to `--filter-rules`. |
| Huge output | `--max-nodes 1000 --collapse-threshold 20`. |

**Help**: `FileTree --help` / `FileTree [command] --help`.

Source: [src/FileTree.CLI](src/FileTree.CLI).

