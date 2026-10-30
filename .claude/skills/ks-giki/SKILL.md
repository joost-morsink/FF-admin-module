---
name: ks-giki
description: Write, review and run `ks` (Knowledge Script) code and work on Giki wikis. Use when editing .ks files, ```ks / val/ks / diagram/ks / table/ks blocks in Markdown, ks.config.json or .giki/config.json, or when running the `ks` and `giki` tools (ks check/run/format, giki zip/export).
---

# ks and Giki

`ks` is a small language that derives RDF graphs from other graphs and checks them (BitWizards.Knowledge.Scripting).
Giki is a Git-backed Markdown wiki that exposes itself as RDF and runs `ks` inside pages. Use this skill to write correct scripts for both.

In this repo the wiki is the `wiki/` folder (scripts such as `wiki/events/Module.ks`, a `prelude.ks`).

## Core model
- Knowledge = a set of triples. Every step takes knowledge and returns new knowledge. Scripts never mutate: `x = y | step`.
- Steps chain with `|` only. 
- Statements run top to bottom; a name must be defined before it is used. `#` starts a comment.

## Script layout
Directives first, each ending in ` .`, then statements:
```
@prefix ex: <https://example.org/> .
@base <https://example.org/> .
@include "lib/defs.ks" .                  # relative to the including script
@fun name this (?node, other) { name = this | extract { ... } } .   # result = variable named like the function
x = { ex:a a ex:Thing }                   # inline knowledge
y = x | assert { ex:a rdfs:label "A"@en }
verify y ask { ex:a rdfs:label "A"@en }
```
`rdf:`, `rdfs:`, `owl:`, `xsd:` are always known.

## Expressions and steps
- `{ triples }`, `name`, `load "path-or-source-name"`, `expr | step`.
- `assert { triples }`: add concrete triples. NO variables (use infer).
- `infer { template } where { pattern }`: add the template per match. `where` is REQUIRED. Runs once, not to a fixpoint (use `p+`/`p*` for transitive).
- `extract { template } where { pattern }`: keep only the template's triples that match. The result holds ONLY the template triples, so put wanted properties in the template. `where` optional.
- `construct { template } where { pattern }`: new knowledge, only the template.
- `retract { template } [where { pattern }]`.
- `enhance { S } with (src) { T } where { P }`: copy facts from another source.
- `merge name` / `merge (expr)`, `load "path"` as a step, `f(args)` / `call f(args)`.

## Triples and patterns
- Turtle-like: `s p o ; p2 o2 , o3 .`, `a` = rdf:type. Literals: `"x"`, `"x"@nl`, `"1"^^xsd:integer`, `1`, `1.5`, `true`. A bare word is an error: quote strings.
- Variables `?x`. Blank nodes `_:e`, `[ p o ]`. Blank nodes have no stable name across statements; use `mint` when you need a nameable node.
- `mint name(args)`: deterministic IRI from the args (`urn:mint:name:hash`). Variables allowed only inside templates; elsewhere args must be concrete. Makes `infer` idempotent.
- Pattern parts: triples (predicate may be a property path `p/q`, `p|q`, `^p`, `p*`, `p+`, `p?`), `from (expr) { ... }`, `not { }`, `optional { }`, `{ } or { }`, `filter (SPARQL expr)`.
- Reserved in patterns: filter, from, not, optional, or, mint, each, has.
- A template triple whose variable is unbound is skipped; wrap in a template `optional { }` for all-or-none.

## verify
```
verify X ask { triples }                 # each missing triple fails
verify X ask not { triples }
verify X is empty | is not empty
verify X equal { ... }
verify X subset of Y
verify X each { ?p a s:DigitalDocument } has { ?p s:name ?n }      # every match of P must match Q
verify X each { ... } has not { ... }
```
Failed verifies are reported (not thrown); unknown variables/functions and syntax errors throw.

## Gotchas
- Directives before statements; `@fun ... } .` ends with a dot.
- `extract` loses everything not in the template.
- Fix unknown-term warnings by checking the spelling against the vocabulary rather than silencing them.
- Namespaces are under `http://bit-wizards.nl/` and end in `/` (e.g. `http://bit-wizards.nl/arch/`); 

## Configuration
`ks.config.json` (or `.giki/ks.config.json`, `.giki/config.json`) is found by walking up from the script; the first folder with one wins, nothing is merged. Comments and trailing commas are allowed. Paths are relative to the file.
Keys: `sources` (names for `load`), `vocabularies`, `prefixes`, `execute` (scripts run before every script: the preamble), `variables`, `wiki.base`, and for Giki `export` / `imports`.

## ks CLI
```sh
ks check file.ks                # parse + analyze
ks format file.ks
ks run file.ks [--format text|github|json|junit|trx|ttl] [--report-file out]   # run from the wiki folder
ks output ...
```
`--format ttl` writes failed validations in the `validation:` vocabulary (http://bit-wizards.nl/ks/validation/); warnings aren't exported by default.
Check `ks --help` for options not listed here.

## In a Giki wiki
- Pages are Markdown. Fenced blocks with `kind/type`:
  - `val/ks` (or `validation/ks`): checks the wiki. Silent when OK; problems are rendered and fail `giki zip` (exit 3).
  - `diagram/ks`: script builds a `diagram:Diagram` graph in variable `diagram`.
  - `table/ks`: script builds a `table:Table` graph in variable `table`. `table/ttl` takes Turtle.
  - Settings go in YAML front matter (`---`) at the top of the block (`result`, `table`, `type`).
- Ambient: variable `wiki` is the read-only graph of the whole wiki. Start with `wiki | extract {...}`. There is NO `load "wiki"` in Giki.
- Pages are `s:DigitalDocument` nodes (`s:name`, `s:mentions`, `s:author`, `s:about`). Prefixes need no declaration: `s` (schema.org), `rdf`, `rdfs`, `xsd`, `diagram`, `plantuml`, `archimate`, `table`.
- `@base` is the page, so `<#d>` is an anchor on the page. `load "x.ttl"` is relative to the page (leading `/` = wiki root). Nothing outside the wiki can be loaded.
- Limits: 5 s per query, 20 s per script including preamble.
- Strict mode checks diagram/archimate/plantuml terms: a typo like `diagram:contain` is a problem, not an empty diagram.
- The `.giki` folder is not content: it holds `config.json`, `prelude.ks` and similar. Preamble = `execute` list; without a config file `.giki/prelude.ks` is used, with a config the `execute` list is the whole truth.

Examples:
````ks
```val/ks
verify wiki each { ?p a s:DigitalDocument } has { ?p s:name ?n }
verify wiki each { ?a a archimate:ApplicationComponent } has { ?a rdfs:comment ?c }
```
````
````ks
```diagram/ks
diagram = wiki
    | infer { <#d> diagram:contains ?p . ?p rdfs:label ?n } where { ?p a s:DigitalDocument . ?p s:name ?n }
    | infer { <#d> diagram:edge mint edge(?a, ?b) . mint edge(?a, ?b) diagram:source ?a ; diagram:target ?b }
      where { ?a s:mentions ?b . ?b a s:DigitalDocument }
    | assert { <#d> a diagram:Diagram ; rdfs:label "Pages" ; diagram:direction diagram:LeftToRight }
```
````
````ks
```table/ks
table = wiki
    | infer { <#t> table:rows ?x } where { ?x a archimate:ApplicationComponent }
    | assert {
        <#t> a table:Table ; rdfs:label "Applications" ;
             table:columns [ rdfs:label "Name" ] .   # one column needs no list
      }
```
````
Table vocabulary: `table:columns` (rdf:List), `table:rows`, per column `table:property`, `table:link`, `table:align`, `table:template` (`{value}`, `{link}`), `table:order`.

## giki CLI
```sh
giki zip [--revspec HEAD] [-o out.zip] [--local] [-g repo] [--problem-report f --problem-format ttl]
giki export [-o x.giki.zip] [--no-site] [--loose] [--deterministic]   # package for importing wikis
giki install ...                                                       # plugins
```
Multi-value options (`--config`, `--set`) go AFTER the command. Exit codes: 0 ok, 1 crash, 2 command could not run (usage, bad revision, unwritable output), 3 content errors (output is still written).
Federation: `imports` in config mounts child wikis; each child's export is a variable named by its import `name` in the parent's scripts. `wiki.base` (absolute IRI ending in `/`) is the wiki's identity.
Check `giki zip --help` for options not listed here.

## Workflow when asked to write ks
1. Find the config (`ks.config.json` / `.giki/`) and the existing vocabularies and prelude before inventing prefixes or functions.
2. Write the script, then run `ks check` on it. For wiki blocks, run `giki zip --local -o $TMPDIR/x.zip` (or `ks run` from the wiki root) and read the problems.
3. Prefer `verify ... each ... has` for rules, so failures name the offending resource.
4. Report which commands you ran and their output; do not claim a script works without running it.
