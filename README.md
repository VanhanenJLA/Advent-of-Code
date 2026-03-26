# Advent of Code

Small .NET tooling and solution workspace for solving [Advent of Code](https://adventofcode.com/) puzzles.

The repository contains:

- `CLI`: a command-line app for bootstrapping a puzzle day, fetching inputs and instructions, saving config, and submitting answers
- `Engine`: the puzzle workflow and Advent of Code HTTP integration
- `Solutions`: per-year, per-day solution folders
- `Engine.Tests`: tests around the engine behavior

## Requirements

- .NET 8 SDK
- An Advent of Code account
- Your Advent of Code `session` cookie value

## First-Time Setup

1. Restore or build the solution:

```pwsh
dotnet build Advent-of-Code.sln
```

2. Save your Advent of Code session cookie:

```pwsh
dotnet run --project CLI -- config --cookie "<your-session-cookie>"
```

This stores the cookie at `~/.aoc/cookie.txt`.

## Basic Usage

Start a new puzzle day:

```pwsh
dotnet run --project CLI -- start --year 2025 --day 1
```

That will:

- scaffold `Solutions/<year>/<day>/Solution.cs`
- fetch and save `input.txt`
- fetch and save `puzzle_instructions.html`

Fetch input again and ignore the local cache:

```pwsh
dotnet run --project CLI -- inputs get --year 2025 --day 1 --force
```

Sync inputs and instructions for all existing solutions in a year:

```pwsh
dotnet run --project CLI -- sync --year 2025
```

Fetch instructions again and ignore the local cache:

```pwsh
dotnet run --project CLI -- instructions get --year 2025 --day 1 --force
```

Submit an answer:

```pwsh
dotnet run --project CLI -- submit 12345 --year 2025 --day 1 --level PartOne
```

Remove scaffolded files for a day:

```pwsh
dotnet run --project CLI -- remove --year 2025 --day 1
```

Remove an entire year:

```pwsh
dotnet run --project CLI -- remove --year 2025
```

## Solution Layout

Solutions live under `Solutions/<year>/<day>/`.

A new day currently contains:

- `Solution.cs`: solution and test template
- `input.txt`: your personal puzzle input
- `puzzle_instructions.html`: saved instructions page

## Notes

- The CLI talks directly to `https://adventofcode.com/`, so fetching input, instructions, and submissions requires a valid cookie and network access.
- Some command defaults are still rough; passing `--year` and `--day` explicitly is the safest way to use the CLI.
- The generated solution template starts with placeholder expected values (`"TBD"`), so new puzzle tests need to be filled in manually.
