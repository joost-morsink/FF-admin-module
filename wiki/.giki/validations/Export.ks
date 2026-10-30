# The checks of the documentation, run by "giki export" and "giki zip" as the export script of the wiki (config.json):
# a failed verify is a problem of the wiki. "wiki" is the graph of the wiki, "code" the graph of the code (prelude.ks).
@include "Events.ks" .
@include "Models.ks" .
@include "Requirements.ks" .

# nothing is added to or removed from the export (the pages and the problems); the script is here for the checks
export = export | extract { ?s ?p ?o }
